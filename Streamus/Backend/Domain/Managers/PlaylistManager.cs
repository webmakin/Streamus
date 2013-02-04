﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Exceptions;
using Streamus.Backend.Dao;
using Streamus.Backend.Domain.Interfaces;
using log4net;

namespace Streamus.Backend.Domain.Managers
{
    public class PlaylistManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IPlaylistDao PlaylistDao { get; set; }
        private IPlaylistItemDao PlaylistItemDao { get; set; }

        public PlaylistManager(IPlaylistDao playlistDao, IPlaylistItemDao playlistItemDao)
        {
            PlaylistDao = playlistDao;
            PlaylistItemDao = playlistItemDao;
        }

        public void Save(Playlist playlist)
        {
            try
            {
                NHibernateSessionManager.Instance.BeginTransaction();
                playlist.ValidateAndThrow();
                PlaylistDao.Save(playlist);
                NHibernateSessionManager.Instance.CommitTransaction();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        //public void UpdatePlaylist(Playlist detachedPlaylist)
        //{
        //    try
        //    {
        //        NHibernateSessionManager.Instance.BeginTransaction();

        //        Playlist playlist = PlaylistDao.Get(detachedPlaylist.Id);
        //        if (playlist == null)
        //        {
        //            throw new Exception("Shouldn't be null inside of UpdatePlaylist");
        //        }

        //        playlist.CopyFromDetached(detachedPlaylist);
        //        playlist.ValidateAndThrow();

        //        NHibernateSessionManager.Instance.CommitTransaction();
        //    }
        //    catch (Exception exception)
        //    {
        //        Logger.Error(exception);
        //        throw;
        //    }
        //}

        public void DeletePlaylistById(Guid id)
        {
            try
            {
                NHibernateSessionManager.Instance.BeginTransaction();
                Playlist playlist = PlaylistDao.Get(id);

                PlaylistDao.Delete(playlist);
                NHibernateSessionManager.Instance.CommitTransaction();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        /// <summary>
        /// Copy all the data from a detached playlistItem collection without breaking NHibernate entity mapping.
        /// </summary>
        /// <param name="playlistId">The playlist to update</param>
        /// <param name="detachedItems">The detached items to take data from and update the playlist with</param>
        public void UpdateItemPosition(Guid playlistId, List<PlaylistItem> detachedItems)
        {
            try
            {
                NHibernateSessionManager.Instance.BeginTransaction();
                Playlist playlist = PlaylistDao.Get(playlistId);

                foreach (PlaylistItem playlistItem in playlist.Items)
                {
                    //  Should always find an item.
                    PlaylistItem detachedItem = detachedItems.First(di => di.Position == playlistItem.Position);
                    playlistItem.CopyFromDetached(detachedItem);
                    PlaylistItemDao.Update(playlistItem);
                }
                
                NHibernateSessionManager.Instance.CommitTransaction();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }

        }

        public void UpdateTitle(Guid playlistId, string title)
        {
            NHibernateSessionManager.Instance.BeginTransaction();
            Playlist playlist = PlaylistDao.Get(playlistId);
            playlist.Title = title;
            PlaylistDao.Update(playlist);
            NHibernateSessionManager.Instance.CommitTransaction();
        }

        public void DeleteItem(Guid playlistId, Guid itemId, Guid userId)
        {
            try
            {
                NHibernateSessionManager.Instance.BeginTransaction();
                Playlist playlist = PlaylistDao.Get(playlistId);

                if (playlist.UserId != userId)
                {
                    const string errorMessage = "The specified playlist is not for the given user.";
                    throw new ApplicationException(errorMessage);
                }

                PlaylistItem playlistItem = playlist.Items.First(item => item.Id == itemId);

                //  Be sure to remove from Playlist first so that cascade doesn't re-save.
                playlist.Items.Remove(playlistItem);

                //  Update all playlistItems positions which would be affected by the remove.
                foreach (PlaylistItem item in playlist.Items.Where(i => i.Position > playlistItem.Position))
                {
                    item.Position--;
                }

                PlaylistDao.Update(playlist);

                NHibernateSessionManager.Instance.CommitTransaction();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public void CreatePlaylistItem(PlaylistItem playlistItem)
        {
            try
            {
                NHibernateSessionManager.Instance.BeginTransaction();
                playlistItem.ValidateAndThrow();
                PlaylistItemDao.Save(playlistItem);
                try
                {
                    NHibernateSessionManager.Instance.CommitTransaction();
                }
                catch (GenericADOException exception)
                {
                    //Got beat to saving this entity. Not sure if this is a big deal or not...
                    Logger.Error(exception);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public void UpdatePlaylistItem(PlaylistItem playlistItem)
        {
            try
            {
                NHibernateSessionManager.Instance.BeginTransaction();
                playlistItem.ValidateAndThrow();

                PlaylistItem knownPlaylistItem = PlaylistItemDao.Get(playlistItem.PlaylistId, playlistItem.Id);

                //  TODO: Sometimes we're updating and sometimes we're creating because the client
                //  sets PlaylistItem's ID so its difficult to tell server-side.
                if (knownPlaylistItem == null)
                {
                    PlaylistItemDao.Save(playlistItem);
                }
                else
                {
                    //  TODO: I don't think I should need both of these, double check at some point.
                    //PlaylistItemDao.Update(playlistItem);
                    PlaylistItemDao.Merge(playlistItem);
                }

                NHibernateSessionManager.Instance.CommitTransaction();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public void CreatePlaylistItems(IEnumerable<PlaylistItem> playlistItems)
        {
            try
            {
                NHibernateSessionManager.Instance.BeginTransaction();

                foreach (PlaylistItem item in playlistItems)
                {
                    item.ValidateAndThrow();
                    //TODO: Optimize into one SQL query.
                    PlaylistItemDao.Save(item);
                }

                NHibernateSessionManager.Instance.CommitTransaction();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }
    }
}
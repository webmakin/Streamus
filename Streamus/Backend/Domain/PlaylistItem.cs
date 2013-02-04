﻿using System;
using System.Runtime.Serialization;
using FluentValidation;
using Streamus.Backend.Domain.Validators;

namespace Streamus.Backend.Domain
{
    [DataContract]
    public class PlaylistItem
    {
        [DataMember(Name = "playlistId")]
        public Guid PlaylistId { get; set; }

        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        [DataMember(Name = "position")]
        public int Position { get; set; }

        //  Store Title on PlaylistItem as well as on Video because user might want to rename PlaylistItem.
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "videoId")]
        public string VideoId { get; set; }

        public PlaylistItem()
        {
            //  Id shall be generated by the client. This is OK because it is composite key with 
            //  PlaylistId which is generated by the server. 
            Id = Guid.Empty;
            PlaylistId = Guid.Empty;
            Position = -1;
            Title = string.Empty;
            VideoId = string.Empty;
        }

        public PlaylistItem(Guid playlistId, Guid id, int position, string title, string videoId)
            : this()
        {
            PlaylistId = playlistId;
            Id = id;
            Position = position;
            Title = title;
            VideoId = videoId;
        }

        /// <summary>
        /// Used to merge details from a detached playlistItem into
        /// a playlistItem already known to NHibernate's Session.
        /// </summary>
        public void CopyFromDetached(PlaylistItem detachedItem)
        {
            //  Don't copy the ID or Playlist's ID because the properties get marked dirty and PK's should never change.
            Position = detachedItem.Position;
            Title = detachedItem.Title;
            VideoId = detachedItem.VideoId;
        }

        public void ValidateAndThrow()
        {
            var validator = new PlaylistItemValidator();
            validator.ValidateAndThrow(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PlaylistItem) obj);
        }

        private bool Equals(PlaylistItem other)
        {
            return PlaylistId.Equals(other.PlaylistId) && Id == other.Id;
        }

        /// <summary>
        ///     Must be provided to properly compare two objects
        ///     http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        /// </summary>
        public override int GetHashCode()
        {
            return (PlaylistId + "|" + Id).GetHashCode();
        }

        //  TODO: Are my current implementations of Equals and GetHashCode OK?
    }
}
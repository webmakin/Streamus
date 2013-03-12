﻿using FluentValidation;

namespace Streamus.Backend.Domain.Validators
{
    public class StreamValidator : AbstractValidator<Stream>
    {
        public StreamValidator()
        {
            RuleFor(stream => stream.Title).Length(0, 255);
        }
    }
}
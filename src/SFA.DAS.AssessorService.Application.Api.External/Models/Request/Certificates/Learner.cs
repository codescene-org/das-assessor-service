﻿using SFA.DAS.AssessorService.Application.Api.External.SwaggerHelpers.Attributes;
using System;

namespace SFA.DAS.AssessorService.Application.Api.External.Models.Request.Certificates
{
    public sealed class Learner : IEquatable<Learner>
    {
        [SwaggerRequired]
        public long Uln { get; set; }
        [SwaggerRequired]
        public string FamilyName { get; set; }

        #region GetHashCode, Equals and IEquatable
        public override int GetHashCode()
        {
            unchecked
            {
                const int hashBase = (int)2166136261;
                const int multiplier = 16777619;

                int hash = hashBase;
                hash = (hash * multiplier) ^ Uln.GetHashCode();
                hash = (hash * multiplier) ^ (FamilyName is null ? 0 : FamilyName.GetHashCode());
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return IsEqual((Learner)obj);
        }

        public bool Equals(Learner other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsEqual(other);
        }

        private bool IsEqual(Learner other)
        {
            return Equals(Uln, other.Uln)
                && string.Equals(FamilyName, other.FamilyName);
        }

        public static bool operator ==(Learner left, Learner right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Learner left, Learner right)
        {
            return !(left == right);
        }
        #endregion
    }
}

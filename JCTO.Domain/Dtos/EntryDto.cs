﻿using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryDto : ConcurrencyHandledDto
    {
        public string EntryNo { get; set; }
        public double InitialQuantity { get; set; }
        public string ToBondNo { get; set; }
        public DateTime EntryDate { get; set; }
        public EntryStatus Status { get; set; }
    }
}

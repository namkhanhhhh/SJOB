﻿using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? PaymentStatus { get; set; }

    public string PaymentType { get; set; } = null!;

    public int? ReferenceId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? VnpayTransactionInfo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}

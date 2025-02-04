﻿using System.ComponentModel.DataAnnotations;

namespace LudereAI.Domain.Models;

public class BaseEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DeletedAt { get; set; } = DateTime.MinValue;
}
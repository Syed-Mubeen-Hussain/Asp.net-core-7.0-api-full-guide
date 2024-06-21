using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Repos.Models;

[Table("RefreshToken")]
public partial class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TokenId { get; set; }

    [Column("RefreshToken")]
    [StringLength(50)]
    [Unicode(false)]
    public string? RefreshToken1 { get; set; }
}

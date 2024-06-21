using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Repos.Models;

public partial class ProductImage
{
    [Key]
    public int Id { get; set; }

    public int? ProductCode { get; set; }

    [Column("ProductImage", TypeName = "image")]
    public byte[]? ProductImage1 { get; set; }
}

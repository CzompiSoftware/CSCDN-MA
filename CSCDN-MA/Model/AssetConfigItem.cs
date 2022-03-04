using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSCDNMA.Model;

public class AssetConfigItem
{
    public int Id { get; set; }

    public Guid ProductId { get; set; }
    public string AssetRoute { get; set; }

    public string RequestRoute { get; set; }

    public virtual Product Product { get; set; }
}

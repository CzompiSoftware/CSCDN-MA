using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSCDNMA.Model;

public class WhitelistItem
{
    [ForeignKey("FK_ProdId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ProductId { get; set; }

    /// <summary>
    /// Regex value is accepted
    /// </summary>
    [Required]
    public string ResponseRoute { get; set; }

    /// <summary>
    /// Regex value is accepted
    /// </summary>
    [Required]
    public string RequestRoute { get; set; }
    public virtual Product Product { get; set; }
}

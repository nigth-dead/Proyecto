using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Categoria
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}

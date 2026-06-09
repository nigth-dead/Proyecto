using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Proyecto.Models;

public partial class punto_de_ventaContext : DbContext
{
    public punto_de_ventaContext()
    {
    }

    public punto_de_ventaContext(DbContextOptions<punto_de_ventaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<CorteCaja> CorteCaja { get; set; }

    public virtual DbSet<DetalleVenta> DetalleVenta { get; set; }

    public virtual DbSet<HistorialMovimiento> HistorialMovimiento { get; set; }

    public virtual DbSet<HistorialPedido> HistorialPedido { get; set; }

    public virtual DbSet<HistorialPedidoDetalle> HistorialPedidoDetalle { get; set; }

    public virtual DbSet<Inventario> Inventario { get; set; }

    public virtual DbSet<Pago> Pago { get; set; }

    public virtual DbSet<Producto> Producto { get; set; }

    public virtual DbSet<Proveedor> Proveedor { get; set; }

    public virtual DbSet<Tienda> Tienda { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    public virtual DbSet<Venta> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySQL("server=localhost;database=punto_de_venta;user=root;password=USS-DF37K");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PRIMARY");

            entity.ToTable("categoria");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CorteCaja>(entity =>
        {
            entity.HasKey(e => e.CorteId).HasName("PRIMARY");

            entity.ToTable("corte_caja");

            entity.HasIndex(e => e.UsuarioId, "fk_corte_usuario");

            entity.HasIndex(e => new { e.TiendaId, e.FechaApertura }, "idx_corte_tienda_fecha");

            entity.Property(e => e.CorteId).HasColumnName("corte_id");
            entity.Property(e => e.Estado)
                .HasColumnType("enum('Completo','Pendiente')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaApertura)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_apertura");
            entity.Property(e => e.FechaCierre)
                .HasColumnType("datetime")
                .HasColumnName("fecha_cierre");
            entity.Property(e => e.SaldoEsperado)
                .HasPrecision(10)
                .HasColumnName("saldo_esperado");
            entity.Property(e => e.SaldoFinal)
                .HasPrecision(10)
                .HasColumnName("saldo_final");
            entity.Property(e => e.SaldoInicial)
                .HasPrecision(10)
                .HasColumnName("saldo_inicial");
            entity.Property(e => e.TiendaId).HasColumnName("tienda_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Tienda).WithMany(p => p.CorteCaja)
                .HasForeignKey(d => d.TiendaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_corte_tienda");

            entity.HasOne(d => d.Usuario).WithMany(p => p.CorteCaja)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_corte_usuario");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.DetalleId).HasName("PRIMARY");

            entity.ToTable("detalle_venta");

            entity.HasIndex(e => e.ProductoId, "fk_detalle_producto");

            entity.HasIndex(e => e.VentaId, "idx_detalle_venta");

            entity.Property(e => e.DetalleId).HasColumnName("detalle_id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Iva)
                .HasPrecision(10)
                .HasColumnName("iva");
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(10)
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Subtotal)
                .HasPrecision(10)
                .HasColumnName("subtotal")
                .ValueGeneratedOnAddOrUpdate();
            entity.Property(e => e.VentaId).HasColumnName("venta_id");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_detalle_producto");

            entity.HasOne(d => d.Venta).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("fk_detalle_venta");
        });

        modelBuilder.Entity<HistorialMovimiento>(entity =>
        {
            entity.HasKey(e => e.MovimientoId).HasName("PRIMARY");

            entity.ToTable("historial_movimiento");

            entity.HasIndex(e => e.InventarioId, "fk_movimiento_inventario");

            entity.HasIndex(e => e.UsuarioId, "fk_movimiento_usuario");

            entity.HasIndex(e => e.Fecha, "idx_movimiento_fecha");

            entity.Property(e => e.MovimientoId).HasColumnName("movimiento_id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.InventarioId).HasColumnName("inventario_id");
            entity.Property(e => e.Motivo)
                .HasMaxLength(100)
                .HasColumnName("motivo");
            entity.Property(e => e.Tipo)
                .HasColumnType("enum('agregar','eliminar','ajuste')")
                .HasColumnName("tipo");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Inventario).WithMany(p => p.HistorialMovimiento)
                .HasForeignKey(d => d.InventarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_movimiento_inventario");

            entity.HasOne(d => d.Usuario).WithMany(p => p.HistorialMovimiento)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_movimiento_usuario");
        });

        modelBuilder.Entity<HistorialPedido>(entity =>
        {
            entity.HasKey(e => e.PedidoId).HasName("PRIMARY");

            entity.ToTable("historial_pedido");

            entity.HasIndex(e => e.TiendaId, "fk_historial_pedido_tienda");

            entity.HasIndex(e => e.ProveedorId, "fk_pedido_proveedor");

            entity.HasIndex(e => e.UsuarioId, "fk_pedido_usuario");

            entity.HasIndex(e => e.Estado, "idx_pedido_estado");

            entity.Property(e => e.PedidoId).HasColumnName("pedido_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'pendiente'")
                .HasColumnType("enum('pendiente','completado','cancelado')")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.MontoTotal)
                .HasPrecision(10)
                .HasColumnName("monto_total");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.TiendaId).HasColumnName("tienda_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.HistorialPedido)
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_pedido_proveedor");

            entity.HasOne(d => d.Tienda).WithMany(p => p.HistorialPedido)
                .HasForeignKey(d => d.TiendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_pedido_tienda");

            entity.HasOne(d => d.Usuario).WithMany(p => p.HistorialPedido)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_pedido_usuario");
        });

        modelBuilder.Entity<HistorialPedidoDetalle>(entity =>
        {
            entity.HasKey(e => e.DetalleId).HasName("PRIMARY");

            entity.ToTable("historial_pedido_detalle");

            entity.HasIndex(e => e.ProductoId, "fk_pedido_det_producto");

            entity.HasIndex(e => new { e.PedidoId, e.ProductoId }, "uq_pedido_producto").IsUnique();

            entity.Property(e => e.DetalleId).HasColumnName("detalle_id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.CostoUnitario)
                .HasPrecision(10)
                .HasColumnName("costo_unitario");
            entity.Property(e => e.PedidoId).HasColumnName("pedido_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Pedido).WithMany(p => p.HistorialPedidoDetalle)
                .HasForeignKey(d => d.PedidoId)
                .HasConstraintName("fk_pedido_det_pedido");

            entity.HasOne(d => d.Producto).WithMany(p => p.HistorialPedidoDetalle)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_pedido_det_producto");
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.InventarioId).HasName("PRIMARY");

            entity.ToTable("inventario");

            entity.HasIndex(e => e.ProductoId, "fk_inventario_producto");

            entity.HasIndex(e => e.Stock, "idx_inventario_stock");

            entity.HasIndex(e => e.TiendaId, "tienda_id_UNIQUE").IsUnique();

            entity.HasIndex(e => new { e.TiendaId, e.ProductoId }, "uq_tienda_producto").IsUnique();

            entity.Property(e => e.InventarioId).HasColumnName("inventario_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.TiendaId).HasColumnName("tienda_id");

            entity.HasOne(d => d.Producto).WithMany(p => p.Inventario)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_inventario_producto");

            entity.HasOne(d => d.Tienda).WithOne(p => p.Inventario)
                .HasForeignKey<Inventario>(d => d.TiendaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_inventario_tienda");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PRIMARY");

            entity.ToTable("pago");

            entity.HasIndex(e => e.VentaId, "idx_pago_venta");

            entity.Property(e => e.PagoId).HasColumnName("pago_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.Metodo)
                .HasDefaultValueSql("'efectivo'")
                .HasColumnType("enum('efectivo','tarjeta','transferencia')")
                .HasColumnName("metodo");
            entity.Property(e => e.Monto)
                .HasPrecision(10)
                .HasColumnName("monto");
            entity.Property(e => e.Procesado).HasColumnName("procesado");
            entity.Property(e => e.VentaId).HasColumnName("venta_id");

            entity.HasOne(d => d.Venta).WithMany(p => p.Pago)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_pago_venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PRIMARY");

            entity.ToTable("producto");

            entity.HasIndex(e => e.CategoriaId, "fk_producto_categoria");

            entity.HasIndex(e => e.ProveedorId, "idx_producto_proveedor");

            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasPrecision(10)
                .HasColumnName("precio");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Producto)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_producto_categoria");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.Producto)
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_producto_proveedor");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.ProveedorId).HasName("PRIMARY");

            entity.ToTable("proveedor");

            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Correo)
                .HasMaxLength(30)
                .HasColumnName("correo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Tienda>(entity =>
        {
            entity.HasKey(e => e.TiendaId).HasName("PRIMARY");

            entity.ToTable("tienda");

            entity.Property(e => e.TiendaId).HasColumnName("tienda_id");
            entity.Property(e => e.Direccion)
                .HasMaxLength(50)
                .HasColumnName("direccion");
            entity.Property(e => e.Estado)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasComment("1=activa, 0=inactiva")
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.TiendaId, "idx_usuario_tienda");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .HasComment("Guardar como hash (bcrypt/argon2)")
                .HasColumnName("contrasena");
            entity.Property(e => e.Contratado)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("contratado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasDefaultValueSql("'Cajero'")
                .HasColumnType("enum('Administrador','Cajero')")
                .HasColumnName("rol");
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("telefono");
            entity.Property(e => e.TiendaId).HasColumnName("tienda_id");
            entity.Property(e => e.Trabajando)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("trabajando");

            entity.HasOne(d => d.Tienda).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.TiendaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_usuario_tienda");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.VentaId).HasName("PRIMARY");

            entity.ToTable("venta");

            entity.HasIndex(e => e.UsuarioId, "fk_venta_usuario");

            entity.HasIndex(e => e.CorteId, "idx_venta_corte");

            entity.HasIndex(e => e.Fecha, "idx_venta_fecha");

            entity.Property(e => e.VentaId).HasColumnName("venta_id");
            entity.Property(e => e.CorteId).HasColumnName("corte_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.TiendaId).HasColumnName("tienda_id");
            entity.Property(e => e.Total)
                .HasPrecision(10)
                .HasColumnName("total");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Corte).WithMany(p => p.Venta)
                .HasForeignKey(d => d.CorteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_venta_corte");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Venta)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_venta_usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

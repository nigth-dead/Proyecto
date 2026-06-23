// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
$(document).ready(function () {
    $('.tabla-mayor-menor').each(function () {
        if (!DataTable.isDataTable(this)) {
            
            const textoVacio = this.dataset.empty || "No hay registros disponibles";

            new DataTable(this, {
                paging: false,
                searching: false,
                info: false,
                ordering: true,
                autoWidth: false,
                scrollX: false,
                order: [[0, 'desc']],
                language: {
                    emptyTable: textoVacio,
                },
                colResize: {
                    isEnabled: true,
                    saveState: false
                },
                columnDefs: [
                    {
                        orderable: false,
                        targets: 'no-ordenar'
                    }
                ]
            });
        }
    });
});

$(document).ready(function () {
    $('.tabla-menor-mayor').each(function () {
        if (!DataTable.isDataTable(this)) {
            
            const textoVacio = this.dataset.empty || "No hay registros disponibles";

            new DataTable(this, {
                paging: false,
                searching: false,
                info: false,
                ordering: true,
                autoWidth: false,
                scrollX: false,
                order: [[0, 'asc']],
                language: {
                    emptyTable: textoVacio,
                },
                colResize: {
                    isEnabled: true,
                    saveState: false
                },
                columnDefs: [
                    {
                        orderable: false,
                        targets: 'no-ordenar'
                    }
                ]
            });
        }
    });
});


// Write your JavaScript code.
function seleccionarUsuario(nombre) {
    document.getElementById("Nombre").value = nombre;
}

function agregarProducto() {
    const contenedor = document.getElementById("productosPedido");
    const primerProducto = contenedor.querySelector(".producto-pedido");
    const nuevoProducto = primerProducto.cloneNode(true);
    nuevoProducto.querySelectorAll("input").forEach(input => input.value = "");
    nuevoProducto.querySelectorAll("select").forEach(select => select.selectedIndex = 0);
    contenedor.appendChild(nuevoProducto);
    actualizarProductos();
}

function calcularTotal(){
    let total = 0;
    const cantidades = document.getElementsByName("Cantidades");
    const costos = document.getElementsByName("CostosUnitarios");
    for (let i = 0; i < cantidades.length; i++){
        const cantidad = cantidades[i].value || 0;
        const costo = costos[i].value || 0;
        total += cantidad * costo;
    }
    document.getElementById("montoTotal").value = "$" + total.toFixed(2);
}

function actualizarProductos() {
    const selects =
        document.querySelectorAll('select[name="ProductoIds"]');
    const seleccionados = [];
    selects.forEach(select => {
        if (select.value !== "") {
            seleccionados.push(select.value);
        }
    });
    selects.forEach(select => {
        const valorActual = select.value;
        Array.from(select.options).forEach(option => {
            if (option.value === "") return;
            option.disabled =
                seleccionados.includes(option.value)
                && option.value !== valorActual;
        });
    });
}

function eliminarProducto(boton) {
    const productos = document.querySelectorAll(".producto-pedido");

    if (productos.length > 1) {
        boton.closest(".producto-pedido").remove();
    } else {
        const producto = boton.closest(".producto-pedido");

        producto.querySelector('select[name="ProductoIds"]').selectedIndex = 0;
        producto.querySelector('input[name="Cantidades"]').value = "";
        producto.querySelector('input[name="CostosUnitarios"]').value = "";
    }

    calcularTotal();
    actualizarProductos();
}

function filtrarProductosPorProveedor() {
    const proveedorSeleccionado = document.getElementById("proveedorId").value;
    const selectsProductos = document.querySelectorAll(".select-producto");

    selectsProductos.forEach(select => {
        select.disabled = false;
        select.value = "";

        const opciones = select.querySelectorAll("option");

        opciones.forEach(opcion => {
            const proveedorProducto = opcion.dataset.proveedorId;

            if (!proveedorProducto) {
                opcion.hidden = false;
                opcion.disabled = false;
                opcion.textContent = "Seleccione un producto";
                return;
            }

            if (proveedorProducto === proveedorSeleccionado) {
                opcion.hidden = false;
                opcion.disabled = false;
                opcion.style.display = "";
            } else {
                opcion.hidden = true;
                opcion.disabled = true;
                opcion.style.display = "none";
            }
        });
    });

    actualizarProductos();
    calcularTotal();
}

document.addEventListener("DOMContentLoaded", function () {
    const modalAutomatico = document.querySelector('[data-auto-open="true"]');

    if (modalAutomatico) {
        const modal = new bootstrap.Modal(modalAutomatico);
        modal.show();
        return;
    }

    const modalMensaje = document.getElementById("modalMensaje");

    if (modalMensaje) {
        const modal = new bootstrap.Modal(modalMensaje);
        modal.show();
    }
});

document.addEventListener("hidden.bs.modal", function () {
    if (document.querySelectorAll(".modal.show").length === 0) {
        document.body.classList.remove("modal-open");
        document.body.style.removeProperty("overflow");
        document.body.style.removeProperty("padding-right");

        document.querySelectorAll(".modal-backdrop").forEach(backdrop => {
            backdrop.remove();
        });
    }
});


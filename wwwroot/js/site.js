// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
$(document).ready(function () {
    $('.tabla-dinamica').each(function () {
        if (!DataTable.isDataTable(this)) {
            new DataTable(this, {
                paging: false,
                searching: false,
                info: false,
                ordering: true,
                autoWidth: false,
                scrollX: false,
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

function seleccionarUsuario(nombre) {
    document.getElementById("Nombre").value = nombre;
}

function seleccionarUsuario(nombre) {
    document.getElementById("Nombre").value = nombre;
}


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
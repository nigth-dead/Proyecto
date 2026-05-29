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
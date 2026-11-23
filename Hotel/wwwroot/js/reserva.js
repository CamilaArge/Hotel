// Reservas: buscador por número (modal) y cálculo del monto antes de confirmar (RESERVAS-CP-004)
(function () {
    'use strict';

    function colones(monto) {
        return '₡' + monto.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // ---------- Buscador de reserva por número ----------
    var btnBuscar = document.getElementById('btnBuscarReserva');
    if (btnBuscar) {
        var entrada = document.getElementById('idReservacion');
        var mensaje = document.getElementById('mensajeBusqueda');
        var modalEl = document.getElementById('modalReserva');

        var buscar = function () {
            mensaje.textContent = '';
            var id = parseInt(entrada.value, 10);

            if (!id || id < 1) {
                mensaje.textContent = 'Ingrese un número de reservación válido.';
                return;
            }

            fetch(btnBuscar.dataset.url + '?idReservacion=' + encodeURIComponent(id), { credentials: 'same-origin' })
                .then(function (r) { return r.ok ? r.text() : ''; })
                .then(function (html) {
                    if (!html || !html.trim()) {
                        mensaje.textContent = 'Estimado usuario, no se ha encontrado la reservación, favor realice una.';
                        return;
                    }
                    modalEl.querySelector('.modal-body').innerHTML = html;
                    bootstrap.Modal.getOrCreateInstance(modalEl).show();
                })
                .catch(function () {
                    mensaje.textContent = 'No se pudo consultar la reservación. Intente de nuevo.';
                });
        };

        btnBuscar.addEventListener('click', buscar);
        entrada.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); buscar(); }
        });
    }

    // ---------- Estimado del monto en el formulario de reserva ----------
    var form = document.getElementById('formReserva');
    if (form) {
        var selHabitacion = document.getElementById('selHabitacion');
        var selPaquete = document.getElementById('selPaquete');
        var fEntrada = document.getElementById('fechaEntrada');
        var fSalida = document.getElementById('fechaSalida');
        var personas = form.querySelector('[name="CantidadPersonas"]');
        var detalle = document.getElementById('detalleMonto');
        var total = document.getElementById('totalMonto');

        var precioDe = function (el) {
            if (!el) return 0;
            var origen = el.tagName === 'SELECT' ? el.options[el.selectedIndex] : el;
            return parseFloat(origen && origen.dataset.precio ? origen.dataset.precio : '0') || 0;
        };

        var calcular = function () {
            // Capacidad máxima según la habitación seleccionada (solo en el select del personal)
            if (selHabitacion && selHabitacion.tagName === 'SELECT' && personas) {
                var cap = parseInt(selHabitacion.options[selHabitacion.selectedIndex].dataset.capacidad || '0', 10);
                if (cap > 0) { personas.max = cap; } else { personas.removeAttribute('max'); }
            }

            var precioNoche = precioDe(selHabitacion);
            var precioPaquete = precioDe(selPaquete);
            var e = fEntrada && fEntrada.value ? new Date(fEntrada.value + 'T00:00:00') : null;
            var s = fSalida && fSalida.value ? new Date(fSalida.value + 'T00:00:00') : null;
            var noches = e && s ? Math.round((s - e) / 86400000) : 0;

            if (noches <= 0 || precioNoche <= 0) {
                detalle.textContent = noches <= 0 && e && s
                    ? 'La fecha de salida debe ser posterior a la de entrada.'
                    : 'Seleccione la habitación y las fechas para calcular el monto.';
                total.textContent = colones(0);
                return;
            }

            var texto = noches + ' noche(s) × ' + colones(precioNoche);
            if (precioPaquete > 0) { texto += ' + paquete ' + colones(precioPaquete); }
            detalle.textContent = texto;
            total.textContent = colones(noches * precioNoche + precioPaquete);
        };

        [selHabitacion, selPaquete, fEntrada, fSalida].forEach(function (el) {
            if (el) { el.addEventListener('change', calcular); el.addEventListener('input', calcular); }
        });
        calcular();
    }
})();

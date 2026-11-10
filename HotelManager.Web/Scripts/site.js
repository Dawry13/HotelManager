// Comportamiento comun de la capa de presentacion.
(function () {
    'use strict';

    // Menu lateral en pantallas pequenas
    var sidebar = document.getElementById('sidebar');
    var backdrop = document.getElementById('sidebarBackdrop');
    var menuBtn = document.getElementById('menuBtn');
    function toggleMenu(abrir) {
        if (!sidebar) return;
        sidebar.classList.toggle('open', abrir);
        if (backdrop) backdrop.classList.toggle('show', abrir);
    }
    if (menuBtn) menuBtn.addEventListener('click', function () { toggleMenu(!sidebar.classList.contains('open')); });
    if (backdrop) backdrop.addEventListener('click', function () { toggleMenu(false); });

    // Listas en cascada: <select data-cascade-url="/Bloques/PorHotel" data-cascade-target="#IdBloque">
    // Al cambiar el hotel se recargan las opciones del select destino.
    document.querySelectorAll('select[data-cascade-url]').forEach(function (origen) {
        origen.addEventListener('change', function () {
            var destino = document.querySelector(origen.dataset.cascadeTarget);
            if (!destino) return;

            var vacio = destino.dataset.placeholder || '-- Seleccione --';
            destino.innerHTML = '<option value="">' + vacio + '</option>';
            if (!origen.value) return;

            var url = origen.dataset.cascadeUrl + '?' + origen.dataset.cascadeParam + '=' + encodeURIComponent(origen.value);
            fetch(url, { headers: { 'Accept': 'application/json' } })
                .then(function (r) { return r.json(); })
                .then(function (items) {
                    items.forEach(function (i) {
                        var op = document.createElement('option');
                        op.value = i.id;
                        op.textContent = i.texto;
                        destino.appendChild(op);
                    });
                    if (items.length === 0) {
                        destino.innerHTML = '<option value="">(sin registros activos)</option>';
                    }
                });
        });
    });

    // Al elegir un tipo de habitacion se proponen capacidad y tarifa base.
    var tipo = document.querySelector('select[data-autollenar]');
    if (tipo) {
        tipo.addEventListener('change', function () {
            var op = tipo.options[tipo.selectedIndex];
            if (!op || !op.dataset.capacidad) return;
            var capacidad = document.getElementById('Capacidad');
            var tarifa = document.getElementById('Tarifa');
            var ayuda = document.getElementById('ayudaTipo');
            if (capacidad) { capacidad.value = op.dataset.capacidad; capacidad.max = op.dataset.capacidad; }
            if (tarifa) tarifa.value = op.dataset.tarifa;
            if (ayuda) ayuda.textContent = 'Capacidad máxima del tipo: ' + op.dataset.capacidad + ' huésped(es).';
        });
    }

    // Cotizacion en vivo del formulario de reserva (RN-013).
    var cotizacion = document.getElementById('cotizacion');
    if (cotizacion) {
        var total = document.getElementById('cotizacionTotal');
        var detalle = document.getElementById('cotizacionDetalle');
        var cotizar = function () {
            var hab = document.getElementById('IdHabitacion');
            var ent = document.getElementById('Entrada');
            var sal = document.getElementById('Salida');
            if (!hab || !hab.value || !ent.value || !sal.value) {
                total.textContent = '—';
                detalle.textContent = 'Seleccione habitación y fechas.';
                return;
            }
            var url = cotizacion.dataset.url + '?idHabitacion=' + encodeURIComponent(hab.value) +
                '&entrada=' + encodeURIComponent(ent.value) + '&salida=' + encodeURIComponent(sal.value);
            fetch(url, { headers: { 'Accept': 'application/json' } })
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    if (d.ok) {
                        total.textContent = d.texto;
                        detalle.textContent = d.noches + ' noche(s) con la tarifa vigente de cada día.';
                    } else {
                        total.textContent = '—';
                        detalle.textContent = d.mensaje;
                    }
                });
        };
        document.querySelectorAll('[data-cotizar]').forEach(function (el) { el.addEventListener('change', cotizar); });
        var ent = document.getElementById('Entrada'), sal = document.getElementById('Salida');
        if (ent && sal) {
            ent.addEventListener('change', function () {
                // La salida se mueve al dia siguiente si queda antes de la entrada
                if (ent.value && (!sal.value || sal.value <= ent.value)) {
                    var d = new Date(ent.value + 'T00:00:00'); d.setDate(d.getDate() + 1);
                    sal.value = d.toISOString().slice(0, 10);
                }
                sal.min = ent.value;
                cotizar();
            });
        }
        cotizar();
    }

    // Filtros que se envian solos al cambiar.
    document.querySelectorAll('select[data-autosubmit]').forEach(function (s) {
        s.addEventListener('change', function () { s.form.submit(); });
    });
})();

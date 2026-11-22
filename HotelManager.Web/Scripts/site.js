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

    // Al elegir un tipo de habitacion se propone la capacidad (el precio lo asigna el administrador).
    var tipo = document.querySelector('select[data-autollenar]');
    if (tipo) {
        tipo.addEventListener('change', function () {
            var op = tipo.options[tipo.selectedIndex];
            if (!op || !op.dataset.capacidad) return;
            var capacidad = document.getElementById('Capacidad');
            var ayuda = document.getElementById('ayudaTipo');
            if (capacidad) { capacidad.value = op.dataset.capacidad; capacidad.max = op.dataset.capacidad; }
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

    // ---------- Calendario: no se permiten fechas pasadas ----------
    function hoyIso() {
        var d = new Date();
        d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
        return d.toISOString().slice(0, 10);
    }
    function diaSiguiente(iso) {
        var d = new Date(iso + 'T00:00:00');
        d.setDate(d.getDate() + 1);
        d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
        return d.toISOString().slice(0, 10);
    }
    function validarFecha(input) {
        var min = input.min || hoyIso();
        if (input.value && input.value < min) {
            input.setCustomValidity(input.value < hoyIso()
                ? 'No se permiten fechas pasadas. Elija hoy o una fecha posterior.'
                : 'La salida debe ser posterior a la entrada.');
        } else {
            input.setCustomValidity('');
        }
    }
    document.querySelectorAll('input[type="date"][data-sin-pasado]').forEach(function (input) {
        if (!input.min || input.min < hoyIso()) input.min = hoyIso();
        validarFecha(input);
        input.addEventListener('input', function () { validarFecha(input); });
        input.addEventListener('change', function () { validarFecha(input); if (!input.checkValidity()) input.reportValidity(); });
    });
    // Pares entrada/salida: la salida siempre al menos un dia despues de la entrada.
    function enlazarRango(entrada, salida) {
        var ajustar = function () {
            if (!entrada.value) return;
            salida.min = diaSiguiente(entrada.value);
            if (!salida.value || salida.value <= entrada.value) salida.value = diaSiguiente(entrada.value);
            validarFecha(salida);
        };
        entrada.addEventListener('change', ajustar);
        ajustar();
    }
    document.querySelectorAll('form').forEach(function (form) {
        var ent = form.querySelector('input[type="date"][data-rango="entrada"]');
        var sal = form.querySelector('input[type="date"][data-rango="salida"]');
        if (ent && sal) enlazarRango(ent, sal);
    });

    // ---------- Ventanas emergentes: precargar campos desde el boton que las abre ----------
    // <button data-bs-toggle="modal" data-bs-target="#modalReserva" data-set-habitacion="5" data-set-entrada="2026-12-01">
    var camposModal = { habitacion: 'IdHabitacion', entrada: 'Entrada', salida: 'Salida', huespedes: 'CantidadHuespedes', piso: 'IdPiso', bloque: 'pBloque' };
    document.querySelectorAll('.modal').forEach(function (modal) {
        modal.addEventListener('show.bs.modal', function (ev) {
            var origen = ev.relatedTarget;
            if (!origen) return;
            Object.keys(camposModal).forEach(function (clave) {
                var valor = origen.getAttribute('data-set-' + clave);
                if (valor === null) return;
                var campo = modal.querySelector('#' + camposModal[clave]);
                if (!campo) return;
                campo.value = valor;
                campo.dispatchEvent(new Event('change', { bubbles: true }));
            });
        });
        modal.addEventListener('shown.bs.modal', function () {
            var primero = modal.querySelector('.modal-body input:not([type="hidden"]):not([readonly]), .modal-body select');
            if (primero) primero.focus();
        });
    });

    // Enlaces "Elegir fechas": llevan al buscador y enfocan el campo indicado.
    document.querySelectorAll('[data-enfocar]').forEach(function (a) {
        a.addEventListener('click', function (e) {
            var campo = document.querySelector(a.getAttribute('data-enfocar'));
            if (!campo) return;
            e.preventDefault();
            campo.scrollIntoView({ behavior: 'smooth', block: 'center' });
            setTimeout(function () { campo.focus(); if (campo.showPicker) { try { campo.showPicker(); } catch (x) { } } }, 350);
        });
    });

    // Filtros que se envian solos al cambiar.
    document.querySelectorAll('select[data-autosubmit]').forEach(function (s) {
        s.addEventListener('change', function () { s.form.submit(); });
    });

    // ---------- Campana del personal: avisa de solicitudes de reserva nuevas ----------
    // Cada 30 segundos consulta cuantas solicitudes hay por aprobar; si llego una nueva,
    // actualiza el contador y muestra un aviso emergente.
    var campana = document.getElementById('campanaSolicitudes');
    if (campana && window.fetch) {
        var ultimoVisto = parseInt(campana.dataset.ultimo || '0', 10);
        var contador = document.getElementById('campanaContador');
        var aviso = document.getElementById('avisoSolicitud');
        var consultar = function () {
            if (document.hidden) return;
            fetch(campana.dataset.url, { headers: { 'Accept': 'application/json' }, credentials: 'same-origin' })
                .then(function (r) { return r.ok ? r.json() : null; })
                .then(function (d) {
                    if (!d) return;
                    if (contador) {
                        contador.textContent = d.total > 99 ? '99+' : d.total;
                        contador.classList.toggle('d-none', d.total === 0);
                    }
                    if (d.ultimoId > ultimoVisto) {
                        var nuevas = d.ultimoId - ultimoVisto;
                        ultimoVisto = d.ultimoId;
                        var texto = document.getElementById('avisoSolicitudTexto');
                        if (texto) texto.textContent = 'Hay ' + d.total + ' solicitud(es) por aprobar.' + (nuevas > 0 ? ' Acaba de llegar una nueva.' : '');
                        if (aviso && window.bootstrap) bootstrap.Toast.getOrCreateInstance(aviso).show();
                    }
                })
                .catch(function () { /* sin conexion: se intenta en la siguiente vuelta */ });
        };
        setInterval(consultar, 30000);
        document.addEventListener('visibilitychange', function () { if (!document.hidden) consultar(); });
    }
})();

﻿using System;
using System.Collections.Generic;
using System.Text;
using Contpaqi.SistemasComerciales.Sdk.Extras.Interfaces;
using Contpaqi.SistemasComerciales.Sdk.Extras.Modelos;

namespace Contpaqi.SistemasComerciales.Sdk.Extras.Servicios
{
    public class ServicioMovimientoComercial
    {
        private readonly ServicioErrorComercial _errorComercialServicio;
        private readonly ISistemasComercialesSdk _sdk;
        private readonly ServicioAlmacenComercial _servicioAlmacenComercial;
        private readonly ServicioProductoComercial _servicioProductoComercial;
        private readonly ServicioValorClasificacionComercial _servicioValorClasificacionComercial;

        public ServicioMovimientoComercial(ISistemasComercialesSdk sdk)
        {
            _sdk = sdk;
            _servicioProductoComercial = new ServicioProductoComercial(sdk);
            _errorComercialServicio = new ServicioErrorComercial(sdk);
            _servicioAlmacenComercial = new ServicioAlmacenComercial(sdk);
            _servicioValorClasificacionComercial = new ServicioValorClasificacionComercial(sdk);
        }

        public tMovimiento ExtraerTMovimiento(MovimientoComercial movimiento)
        {
            var nuevoTMovimiento = new tMovimiento();
            nuevoTMovimiento.aConsecutivo = movimiento.Consecutivo;
            nuevoTMovimiento.aUnidades = movimiento.Unidades;
            nuevoTMovimiento.aPrecio = movimiento.Precio;
            nuevoTMovimiento.aCosto = movimiento.Costo;
            nuevoTMovimiento.aCodProdSer = movimiento.CodigoProducto;
            nuevoTMovimiento.aCodAlmacen = movimiento.CodigoAlmacen;
            nuevoTMovimiento.aReferencia = movimiento.Referencia;
            nuevoTMovimiento.aCodClasificacion = movimiento.CodigoValorClasificacion;
            return nuevoTMovimiento;
        }

        public List<MovimientoComercial> TraerMovimientos()
        {
            var movimientosList = new List<MovimientoComercial>();
            _errorComercialServicio.ResultadoSdk = _sdk.fPosPrimerMovimiento();
            movimientosList.Add(LeerDatosMovimientoActual());
            while (_sdk.fPosSiguienteMovimiento() == 0)
            {
                movimientosList.Add(LeerDatosMovimientoActual());
                if (_sdk.fPosMovimientoEOF() == 1)
                {
                    break;
                }
            }
            return movimientosList;
        }

        public List<MovimientoComercial> TraerMovimientos(int idDocumento)
        {
            var movimientosList = new List<MovimientoComercial>();
            if (_sdk.fSetFiltroMovimiento(idDocumento) == 0)
            {
                _errorComercialServicio.ResultadoSdk = _sdk.fPosPrimerMovimiento();
                movimientosList.Add(LeerDatosMovimientoActual());
                while (_sdk.fPosSiguienteMovimiento() == 0)
                {
                    movimientosList.Add(LeerDatosMovimientoActual());
                    if (_sdk.fPosMovimientoEOF() == 1)
                    {
                        break;
                    }
                }
            }
            _errorComercialServicio.ResultadoSdk = _sdk.fCancelaFiltroMovimiento();
            return movimientosList;
        }

        private MovimientoComercial LeerDatosMovimientoActual()
        {
            var consecutivo = new StringBuilder(9);
            var unidades = new StringBuilder(9);
            var precio = new StringBuilder(9);
            var costo = new StringBuilder(9);
            var referencia = new StringBuilder(Constantes.kLongReferencia);
            var idValorClasificacion = new StringBuilder(Constantes.kLongCodigo);
            var id = new StringBuilder(12);
            var textoExtra1 = new StringBuilder(Constantes.kLongTextoExtra);
            var productoId = new StringBuilder(Constantes.kLongCodigo);
            var almacenId = new StringBuilder(Constantes.kLongCodigo);
            var observaciones = new StringBuilder(250);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CNUMEROMOVIMIENTO", consecutivo, 9);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CUNIDADES", unidades, 9);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CPRECIO", precio, 9);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CCOSTOCAPTURADO", costo, 9);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CREFERENCIA", referencia, Constantes.kLongReferencia);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CIDVALORCLASIFICACION", idValorClasificacion, Constantes.kLongCodigo);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CIDMOVIMIENTO", id, 12);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CTEXTOEXTRA1", textoExtra1, Constantes.kLongTextoExtra);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CIDPRODUCTO", productoId, Constantes.kLongCodigo);
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("CIDALMACEN", almacenId, Constantes.kLongCodigo); // Lee el id del almacen
            _errorComercialServicio.ResultadoSdk = _sdk.fLeeDatoMovimiento("COBSERVAMOV", observaciones, 250);
            var movimiento = new MovimientoComercial();
            movimiento.Consecutivo = double.TryParse(consecutivo.ToString(), out var _consecutivo) ? Convert.ToInt32(_consecutivo) : 0; // Int.Parse falla por que regresa 1.00
            movimiento.Unidades = double.Parse(unidades.ToString());
            movimiento.Precio = double.Parse(precio.ToString());
            movimiento.Costo = double.Parse(costo.ToString());
            movimiento.Referencia = referencia.ToString();
            movimiento.IdValorClasificacion = int.TryParse(idValorClasificacion.ToString(), out var _idValorClasificacion) ? _idValorClasificacion : 0;
            movimiento.IdMovimiento = int.Parse(id.ToString());
            movimiento.TextoExtra1 = textoExtra1.ToString();
            movimiento.Observaciones = observaciones.ToString();
            movimiento.IdAlmacen = int.Parse(almacenId.ToString());
            movimiento.IdProducto = int.Parse(productoId.ToString());
            movimiento.Producto = _servicioProductoComercial.BuscarProducto(movimiento.IdProducto);
            movimiento.CodigoProducto = movimiento.Producto.CodigoProducto;
            movimiento.Almacen = _servicioAlmacenComercial.BuscarAlmacen(movimiento.IdAlmacen);
            movimiento.CodigoAlmacen = movimiento.Almacen.CodigoAlmacen;
            movimiento.ValorClasificacion = _servicioValorClasificacionComercial.BuscaValorClasificacion(movimiento.IdValorClasificacion);
            movimiento.CodigoValorClasificacion = movimiento.ValorClasificacion.CodigoValorClasificacion;
            return movimiento;
        }
    }
}
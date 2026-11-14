using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace Flujo
{
    public class FacturaFlujo : IFacturaFlujo
    {
        private const string MensajeErrorXml = "No fue posible generar o enviar el archivo XML";

        private readonly IFacturaDA _facturaDA;
        private readonly IUsuarioDA _usuarioDA;
        private readonly IEmpresaDA _empresaDA;
        private readonly IBitacoraDA _bitacoraDA;
        private readonly IEnviarFactura _enviarFactura;
        private readonly IEnviarXmlFactura _enviarXmlFactura;
        private readonly IGeneradorXmlFactura _generadorXml;
        private readonly IGeneradorPdfFactura _generadorPdf;

        public FacturaFlujo(
            IFacturaDA facturaDA,
            IUsuarioDA usuarioDA,
            IEmpresaDA empresaDA,
            IBitacoraDA bitacoraDA,
            IEnviarFactura enviarFactura,
            IEnviarXmlFactura enviarXmlFactura,
            IGeneradorXmlFactura generadorXml,
            IGeneradorPdfFactura generadorPdf)
        {
            _facturaDA = facturaDA;
            _usuarioDA = usuarioDA;
            _empresaDA = empresaDA;
            _bitacoraDA = bitacoraDA;
            _enviarFactura = enviarFactura;
            _enviarXmlFactura = enviarXmlFactura;
            _generadorXml = generadorXml;
            _generadorPdf = generadorPdf;
        }

        public async Task<ActionResult> ObtenerFacturas(int idUsuario, FacturaFiltro? filtro)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var facturas = await _facturaDA.ObtenerFacturas(idEmpresaUsuario.Value, filtro);
            return new OkObjectResult(facturas);
        }

        public async Task<ActionResult> ObtenerFlujoFacturas(int idUsuario, FlujoFacturaFiltro? filtro)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            if (filtro?.IdCotizacion is int idCotizacion && idCotizacion <= 0)
                return new BadRequestObjectResult("El identificador de cotización no es válido");

            var numeroFactura = filtro?.NumeroFactura?.Trim();
            if (string.IsNullOrEmpty(numeroFactura))
                numeroFactura = null;
            else if (numeroFactura.Length > 50)
                return new BadRequestObjectResult("El número de factura no puede superar 50 caracteres");

            var ordenarPor = filtro?.OrdenarPor?.Trim();
            if (string.IsNullOrEmpty(ordenarPor))
                ordenarPor = "id_cotizacion";
            else if (ordenarPor != "id_cotizacion" && ordenarPor != "numeroFactura")
                return new BadRequestObjectResult("El ordenamiento solo admite id_cotizacion o numeroFactura");

            var filtroConsulta = new FlujoFacturaFiltro
            {
                IdCotizacion = filtro?.IdCotizacion,
                NumeroFactura = numeroFactura,
                OrdenarPor = ordenarPor
            };

            var items = (await _facturaDA.ObtenerFlujoFacturas(idEmpresaUsuario.Value, filtroConsulta)).ToList();
            var hayFiltros = filtroConsulta.IdCotizacion.HasValue || filtroConsulta.NumeroFactura != null;

            string? mensaje = null;
            if (items.Count == 0)
            {
                mensaje = hayFiltros
                    ? "No se encontraron resultados que coincidan"
                    : "No hay registros disponibles";
            }

            await _bitacoraDA.RegistrarBitacora(new Bitacora
            {
                IdUsuario = idUsuario,
                IdEmpresa = idEmpresaUsuario.Value,
                TablaAfectada = "FACTURA",
                Accion = "CONSULTAR_FLUJO",
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    idCotizacion = filtroConsulta.IdCotizacion,
                    numeroFactura = filtroConsulta.NumeroFactura,
                    ordenarPor = filtroConsulta.OrdenarPor
                })
            });

            return new OkObjectResult(new FlujoFacturaConsulta
            {
                Mensaje = mensaje,
                Items = items
            });
        }

        public async Task<ActionResult> ObtenerFlujoFactura(int idUsuario, int idFactura)
        {
            if (idFactura <= 0)
                return new BadRequestObjectResult("El identificador de factura no es válido");

            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var factura = await _facturaDA.ObtenerFactura(idEmpresaUsuario.Value, idFactura);

            if (factura == null || !factura.IdCotizacion.HasValue)
                return new NotFoundObjectResult("La factura no existe");

            var filasFlujo = await _facturaDA.ObtenerFlujoFacturas(
                idEmpresaUsuario.Value,
                new FlujoFacturaFiltro
                {
                    IdCotizacion = factura.IdCotizacion,
                    OrdenarPor = "id_cotizacion"
                });

            var resumen = filasFlujo.FirstOrDefault(fila => fila.IdFactura == idFactura);

            if (resumen == null)
                return new NotFoundObjectResult("La factura no existe");

            await _bitacoraDA.RegistrarBitacora(new Bitacora
            {
                IdUsuario = idUsuario,
                IdEmpresa = idEmpresaUsuario.Value,
                TablaAfectada = "FACTURA",
                RegistroId = idFactura.ToString(),
                Accion = "CONSULTAR_FLUJO",
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    consulta = "detalle",
                    idFactura
                })
            });

            return new OkObjectResult(new FlujoFacturaDetalle
            {
                Factura = factura,
                Resumen = resumen
            });
        }

        public async Task<ActionResult> ObtenerReporteFacturacion(int idUsuario, ReporteFacturacionFiltro? filtro)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var periodo = filtro?.Periodo?.Trim();
            if (string.IsNullOrEmpty(periodo))
                periodo = null;
            else if (!DateTime.TryParseExact(periodo, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return new BadRequestObjectResult("El período debe tener el formato YYYY-MM");

            var tipoFactura = filtro?.TipoFactura?.Trim();
            if (string.IsNullOrEmpty(tipoFactura))
                tipoFactura = null;
            else if (tipoFactura.Length > 50)
                return new BadRequestObjectResult("El tipo de factura no puede superar 50 caracteres");

            var ordenarPor = filtro?.OrdenarPor?.Trim();
            if (string.IsNullOrEmpty(ordenarPor))
                ordenarPor = "periodo";
            else if (ordenarPor != "periodo" && ordenarPor != "tipo_factura")
                return new BadRequestObjectResult("El ordenamiento solo admite periodo o tipo_factura");

            var filtroConsulta = new ReporteFacturacionFiltro
            {
                Periodo = periodo,
                TipoFactura = tipoFactura,
                OrdenarPor = ordenarPor
            };

            var resultado = await _facturaDA.ObtenerReporteFacturacion(idEmpresaUsuario.Value, filtroConsulta);
            var hayFiltros = filtroConsulta.Periodo != null || filtroConsulta.TipoFactura != null;

            string? mensaje = null;
            if (resultado.Grupos.Count == 0)
            {
                mensaje = hayFiltros
                    ? "No se encontraron resultados que coincidan"
                    : "No hay registros disponibles";
            }

            await _bitacoraDA.RegistrarBitacora(new Bitacora
            {
                IdUsuario = idUsuario,
                IdEmpresa = idEmpresaUsuario.Value,
                TablaAfectada = "FACTURA",
                Accion = "CONSULTAR_REPORTE",
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    periodo = filtroConsulta.Periodo,
                    tipoFactura = filtroConsulta.TipoFactura,
                    ordenarPor = filtroConsulta.OrdenarPor
                })
            });

            return new OkObjectResult(new ReporteFacturacionConsulta
            {
                Mensaje = mensaje,
                Grupos = resultado.Grupos
            });
        }

        public async Task<ActionResult> ObtenerDetalleReporteFacturacion(int idUsuario, int idFactura)
        {
            if (idFactura <= 0)
                return new BadRequestObjectResult("El identificador de factura no es válido");

            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var detalle = await _facturaDA.ObtenerDetalleReporteFacturacion(idEmpresaUsuario.Value, idFactura);

            if (detalle == null)
                return new NotFoundObjectResult("La factura no existe");

            await _bitacoraDA.RegistrarBitacora(new Bitacora
            {
                IdUsuario = idUsuario,
                IdEmpresa = idEmpresaUsuario.Value,
                TablaAfectada = "FACTURA",
                RegistroId = idFactura.ToString(),
                Accion = "CONSULTAR_REPORTE",
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    consulta = "detalle",
                    idFactura
                })
            });

            return new OkObjectResult(detalle);
        }

        public async Task<ActionResult> ObtenerFactura(int idUsuario, int idFactura)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var factura = await _facturaDA.ObtenerFactura(idEmpresaUsuario.Value, idFactura);

            if (factura == null)
                return new NotFoundObjectResult("La factura no existe");

            return new OkObjectResult(factura);
        }

        public async Task<ActionResult> Confirmar(int idUsuario, int idFactura, byte[]? versionFila)
        {
            return await EjecutarCambioEstado(idUsuario, idFactura, versionFila, _facturaDA.Confirmar);
        }

        public async Task<ActionResult> Restablecer(int idUsuario, int idFactura, byte[]? versionFila)
        {
            return await EjecutarCambioEstado(idUsuario, idFactura, versionFila, _facturaDA.Restablecer);
        }

        public async Task<ActionResult> Inactivar(
            int idUsuario,
            int idFactura,
            byte[]? versionFila,
            string? motivoAnulacion)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            if (versionFila == null || versionFila.Length != 8)
                return new BadRequestObjectResult("La versión de la factura es obligatoria");

            var motivo = motivoAnulacion?.Trim();

            if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 500)
                return new BadRequestObjectResult("El motivo de anulación es obligatorio");

            var resultado = await _facturaDA.Inactivar(
                idEmpresaUsuario.Value,
                idUsuario,
                idFactura,
                versionFila,
                motivo);

            return MapearCambioEstado(resultado);
        }

        public async Task<ActionResult> Reactivar(int idUsuario, int idFactura, byte[]? versionFila)
        {
            return await EjecutarCambioEstado(idUsuario, idFactura, versionFila, _facturaDA.Reactivar);
        }

        public async Task<ActionResult> RegistrarPago(
            int idUsuario,
            int idFactura,
            decimal? monto,
            string? metodoPago)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var metodo = metodoPago?.Trim();

            if (!monto.HasValue
                || monto.Value <= 0
                || string.IsNullOrWhiteSpace(metodo)
                || metodo.Length > 50)
            {
                return new BadRequestObjectResult("Por favor, rellene los campos obligatorios");
            }

            var resultado = await _facturaDA.RegistrarPago(
                idEmpresaUsuario.Value,
                idFactura,
                monto.Value,
                metodo);

            return MapearPagoFactura(resultado);
        }

        public async Task<ActionResult> RegistrarNotaCredito(
            int idUsuario,
            int idFactura,
            string? motivo,
            decimal? monto,
            string? descripcionAdicional)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var motivoNormalizado = motivo?.Trim();
            var descripcion = string.IsNullOrWhiteSpace(descripcionAdicional)
                ? null
                : descripcionAdicional.Trim();

            if (string.IsNullOrWhiteSpace(motivoNormalizado)
                || motivoNormalizado.Length > 500
                || !monto.HasValue
                || monto.Value <= 0
                || (descripcion != null && descripcion.Length > 500))
            {
                return new BadRequestObjectResult("Por favor, rellene los campos obligatorios");
            }

            var resultado = await _facturaDA.RegistrarNotaCredito(
                idEmpresaUsuario.Value,
                idUsuario,
                idFactura,
                motivoNormalizado,
                monto.Value,
                descripcion);

            return MapearNotaCredito(resultado);
        }

        public async Task<ActionResult> Enviar(int idUsuario, int idFactura)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            return await _enviarFactura.Enviar(idUsuario, idEmpresaUsuario.Value, idFactura);
        }

        public async Task<ActionResult> DescargarXml(int idUsuario, int idFactura)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var idEmpresa = idEmpresaUsuario.Value;
            var factura = await _facturaDA.ObtenerFactura(idEmpresa, idFactura);

            if (factura == null)
                return new NotFoundObjectResult("La factura no existe");

            var rechazo = ValidarElegibilidadXml(factura);
            if (rechazo != null)
                return rechazo;

            var empresa = await _empresaDA.ObtenerEmpresaPorId(idEmpresa);
            byte[] xml;

            try
            {
                xml = _generadorXml.Generar(factura, empresa);
            }
            catch (Exception ex)
            {
                await RegistrarErrorXml(idUsuario, idEmpresa, factura.IdFactura, ex);
                return new BadRequestObjectResult(MensajeErrorXml);
            }

            return new FileContentResult(xml, "application/xml")
            {
                FileDownloadName = NombreArchivoXml(factura.NumeroFactura)
            };
        }

        public async Task<ActionResult> EnviarXml(int idUsuario, int idFactura)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            return await _enviarXmlFactura.Enviar(idUsuario, idEmpresaUsuario.Value, idFactura);
        }

        public async Task<ActionResult> DescargarPdf(int idUsuario, int idFactura)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var idEmpresa = idEmpresaUsuario.Value;
            var factura = await _facturaDA.ObtenerFactura(idEmpresa, idFactura);

            if (factura == null)
                return new NotFoundObjectResult("La factura no existe");

            var rechazo = ValidarElegibilidadPdf(factura);
            if (rechazo != null)
                return rechazo;

            var empresa = await _empresaDA.ObtenerEmpresaPorId(idEmpresa);
            var pdf = _generadorPdf.Generar(factura, empresa);

            return new PdfInlineResult(pdf, NombreArchivoPdf(factura.NumeroFactura));
        }

        private static ActionResult? ValidarElegibilidadPdf(FacturaDetalle factura)
        {
            if (string.Equals(factura.Estado, "Borrador", StringComparison.Ordinal))
                return new BadRequestObjectResult("La factura debe estar confirmada antes de generar el documento");

            if (string.Equals(factura.Estado, "Inactiva", StringComparison.Ordinal))
                return new BadRequestObjectResult("No se puede generar el documento de una factura inactiva");

            if (!string.Equals(factura.Estado, "Emitida", StringComparison.Ordinal)
                && !string.Equals(factura.Estado, "Pagada", StringComparison.Ordinal))
            {
                return new BadRequestObjectResult("La factura no se puede generar en el estado actual");
            }

            return null;
        }

        private static ActionResult? ValidarElegibilidadXml(FacturaDetalle factura)
        {
            if (string.Equals(factura.Estado, "Borrador", StringComparison.Ordinal))
                return new BadRequestObjectResult("La factura debe estar confirmada antes de generar el XML");

            if (string.Equals(factura.Estado, "Inactiva", StringComparison.Ordinal))
                return new BadRequestObjectResult("No se puede generar el XML de una factura inactiva");

            if (!string.Equals(factura.Estado, "Emitida", StringComparison.Ordinal)
                && !string.Equals(factura.Estado, "Pagada", StringComparison.Ordinal))
            {
                return new BadRequestObjectResult(MensajeErrorXml);
            }

            return null;
        }

        private static string NombreArchivoXml(string? numeroFactura)
        {
            var numero = string.IsNullOrWhiteSpace(numeroFactura) ? "sin-numero" : numeroFactura.Trim();
            var invalidos = Path.GetInvalidFileNameChars();
            var sanitizado = new string(numero.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());
            return $"Factura_{sanitizado}.xml";
        }

        private static string NombreArchivoPdf(string? numeroFactura)
        {
            var numero = string.IsNullOrWhiteSpace(numeroFactura) ? "sin-numero" : numeroFactura.Trim();
            var invalidos = Path.GetInvalidFileNameChars();
            var sanitizado = new string(numero.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());
            return $"Factura_{sanitizado}.pdf";
        }

        private sealed class PdfInlineResult : ActionResult
        {
            private readonly byte[] _contenido;
            private readonly string _nombreArchivo;

            public PdfInlineResult(byte[] contenido, string nombreArchivo)
            {
                _contenido = contenido;
                _nombreArchivo = nombreArchivo;
            }

            public override async Task ExecuteResultAsync(ActionContext context)
            {
                var response = context.HttpContext.Response;
                response.StatusCode = 200;
                response.ContentType = "application/pdf";
                response.ContentLength = _contenido.Length;
                response.Headers["Content-Disposition"] = $"inline; filename=\"{_nombreArchivo}\"";
                await response.Body.WriteAsync(_contenido);
            }
        }

        private async Task RegistrarErrorXml(int idUsuario, int idEmpresa, int idFactura, Exception ex)
        {
            try
            {
                await _bitacoraDA.RegistrarBitacora(new Bitacora
                {
                    IdUsuario = idUsuario,
                    IdEmpresa = idEmpresa,
                    TablaAfectada = "FACTURA",
                    RegistroId = idFactura.ToString(),
                    Accion = "ERROR_XML",
                    ValorNuevo = JsonSerializer.Serialize(new
                    {
                        etapa = "generacion",
                        tipo = ex.GetType().Name
                    })
                });
            }
            catch
            {
            }
        }

        private async Task<ActionResult> EjecutarCambioEstado(
            int idUsuario,
            int idFactura,
            byte[]? versionFila,
            Func<int, int, int, byte[], Task<FacturaCambioEstadoResultado>> operacion)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            if (versionFila == null || versionFila.Length != 8)
                return new BadRequestObjectResult("La versión de la factura es obligatoria");

            var resultado = await operacion(idEmpresaUsuario.Value, idUsuario, idFactura, versionFila);

            return MapearCambioEstado(resultado);
        }

        private static ActionResult MapearCambioEstado(FacturaCambioEstadoResultado resultado)
        {
            return resultado.Codigo switch
            {
                1 => new OkObjectResult(new
                {
                    resultado.IdFactura,
                    resultado.NumeroFactura,
                    resultado.Estado,
                    resultado.VersionFila,
                    resultado.Mensaje
                }),
                0 => new NotFoundObjectResult(resultado.Mensaje),
                2 => new ConflictObjectResult(resultado.Mensaje),
                _ => new BadRequestObjectResult(resultado.Mensaje)
            };
        }

        private static ActionResult MapearPagoFactura(FacturaPagoResultado resultado)
        {
            return resultado.Codigo switch
            {
                1 => new OkObjectResult(new
                {
                    resultado.Mensaje,
                    resultado.IdFactura,
                    resultado.NumeroFactura,
                    resultado.Estado,
                    resultado.SaldoPendiente,
                    resultado.IdPagoCliente,
                    resultado.VersionFila
                }),
                0 => new NotFoundObjectResult(resultado.Mensaje),
                4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 => new BadRequestObjectResult(resultado.Mensaje),
                _ => new BadRequestObjectResult(resultado.Mensaje)
            };
        }

        private static ActionResult MapearNotaCredito(FacturaNotaCreditoResultado resultado)
        {
            return resultado.Codigo switch
            {
                1 => new OkObjectResult(new
                {
                    resultado.Mensaje,
                    resultado.IdNotaCredito,
                    resultado.NumeroNotaCredito,
                    resultado.IdFactura,
                    resultado.NumeroFactura,
                    resultado.Monto
                }),
                0 => new NotFoundObjectResult(resultado.Mensaje),
                5 or 7 or 9 or 10 or 11 or 12 or 13 => new BadRequestObjectResult(resultado.Mensaje),
                _ => new BadRequestObjectResult(resultado.Mensaje)
            };
        }
    }
}

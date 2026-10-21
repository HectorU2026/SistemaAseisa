using Abstracciones.Modelos;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Servicios
{
    public interface IGeneradorPdfCotizacion
    {
        byte[] Generar(CotizacionResponseDetallada cotizacion);
    }

    public class GeneradorPdfCotizacion : IGeneradorPdfCotizacion
    {
        public byte[] Generar(CotizacionResponseDetallada cotizacion)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Encabezado
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(cotizacion.Empresa.NombreComercial ?? "")
                                    .FontSize(18).Bold();
                                c.Item().Text(cotizacion.Empresa.RazonSocial ?? "")
                                    .FontSize(9);
                                c.Item().Text($"Cédula: {cotizacion.Empresa.CedulaJuridica ?? "-"}")
                                    .FontSize(9);
                                c.Item().Text($"Tel: {cotizacion.Empresa.Telefono ?? "-"}")
                                    .FontSize(9);
                                c.Item().Text(cotizacion.Empresa.Direccion ?? "")
                                    .FontSize(9);
                            });

                            row.ConstantItem(150).Column(c =>
                            {
                                c.Item().AlignRight().Text("COTIZACIÓN").FontSize(16).Bold();
                                c.Item().AlignRight().Text($"#{cotizacion.NumeroCotizacion}").FontSize(12);
                                c.Item().AlignRight().Text($"Fecha: {cotizacion.FechaEmision:dd/MM/yyyy}").FontSize(9);
                                if (cotizacion.FechaVencimiento.HasValue)
                                    c.Item().AlignRight().Text($"Vence: {cotizacion.FechaVencimiento:dd/MM/yyyy}").FontSize(9);
                            });
                        });

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    // Contenido
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(10);

                        // Cliente
                        col.Item().Column(c =>
                        {
                            c.Item().Text("Cliente").FontSize(11).Bold();
                            c.Item().Text(cotizacion.Cliente.NombreRazonSocial ?? "");
                            c.Item().Text($"Identificación: {cotizacion.Cliente.Identificacion ?? "-"}");
                            if (!string.IsNullOrEmpty(cotizacion.Cliente.Correo))
                                c.Item().Text($"Correo: {cotizacion.Cliente.Correo}");
                            if (!string.IsNullOrEmpty(cotizacion.Cliente.Telefono))
                                c.Item().Text($"Teléfono: {cotizacion.Cliente.Telefono}");
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);      // Código
                                columns.RelativeColumn(3);       // Producto
                                columns.ConstantColumn(50);      // Cantidad
                                columns.ConstantColumn(70);      // Precio
                                columns.ConstantColumn(50);      // Desc %
                                columns.ConstantColumn(50);      // Imp %
                                columns.ConstantColumn(70);      // Subtotal
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Código").Bold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Producto").Bold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Cant.").Bold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Precio").Bold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Desc %").Bold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Imp %").Bold().FontSize(9);
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Subtotal").Bold().FontSize(9);
                            });

                            foreach (var detalle in cotizacion.Detalles)
                            {
                                table.Cell().Padding(5).Text(detalle.CodigoProducto ?? "-").FontSize(9);
                                table.Cell().Padding(5).Text(detalle.NombreProducto ?? "-").FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text(detalle.Cantidad.ToString("N2")).FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text(detalle.PrecioUnitario.ToString("N2")).FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text((detalle.PorcentajeDescuento ?? 0).ToString("N2")).FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text((detalle.PorcentajeImpuesto ?? 0).ToString("N2")).FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text(detalle.Subtotal.ToString("N2")).FontSize(9);
                            }
                        });

                        // Totales
                        col.Item().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Subtotal: {cotizacion.Subtotal:N2} {cotizacion.Moneda}");
                            c.Item().Text($"Descuento: {cotizacion.Descuento:N2} {cotizacion.Moneda}");
                            c.Item().Text($"Impuesto: {cotizacion.Impuesto:N2} {cotizacion.Moneda}");
                            c.Item().Text($"Total: {cotizacion.Total:N2} {cotizacion.Moneda}")
                                .FontSize(12).Bold();
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Generado el ");
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });
            }).GeneratePdf();
        }
    }
}
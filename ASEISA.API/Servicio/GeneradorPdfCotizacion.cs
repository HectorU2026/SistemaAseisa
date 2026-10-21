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
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    // --- ENCABEZADO ---
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(cotizacion.Empresa.NombreComercial ?? "ASEISA")
                                    .FontSize(24).Bold().FontColor("#ff4831"); // Color corporativo
                                c.Item().Text(cotizacion.Empresa.RazonSocial ?? "").FontSize(9);
                                c.Item().Text($"Cédula: {cotizacion.Empresa.CedulaJuridica ?? "-"}").FontSize(9);
                                c.Item().Text($"Tel: {cotizacion.Empresa.Telefono ?? "-"}").FontSize(9);
                                c.Item().Text(cotizacion.Empresa.Direccion ?? "").FontSize(9);
                            });

                            row.ConstantItem(150).Column(c =>
                            {
                                c.Item().AlignRight().Text("COTIZACIÓN").FontSize(16).Bold().FontColor(Colors.Grey.Darken2);
                                c.Item().AlignRight().Text($"#{cotizacion.NumeroCotizacion}").FontSize(14).Bold();
                                c.Item().AlignRight().Text($"Fecha: {cotizacion.FechaEmision:dd/MM/yyyy}").FontSize(9);
                                if (cotizacion.FechaVencimiento.HasValue)
                                    c.Item().AlignRight().Text($"Vence: {cotizacion.FechaVencimiento:dd/MM/yyyy}").FontSize(9);
                            });
                        });

                        col.Item().PaddingTop(10).LineHorizontal(2).LineColor("#ff4831");
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(c =>
                        {
                            c.Spacing(6); 

                            c.Item().Text("DATOS DEL CLIENTE").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                            c.Item().Text(cotizacion.Cliente.NombreRazonSocial ?? "").FontSize(13).Bold();
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

                            // Encabezado de tabla en gris oscuro
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Darken3).Padding(6).Text("Código").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background(Colors.Grey.Darken3).Padding(6).Text("Producto").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Cant.").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Precio").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Desc %").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Imp %").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background(Colors.Grey.Darken3).Padding(6).AlignRight().Text("Subtotal").Bold().FontColor(Colors.White).FontSize(9);
                            });

                            foreach (var detalle in cotizacion.Detalles)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(detalle.CodigoProducto ?? "-").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(detalle.NombreProducto ?? "-").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(detalle.Cantidad.ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(detalle.PrecioUnitario.ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text((detalle.PorcentajeDescuento ?? 0).ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text((detalle.PorcentajeImpuesto ?? 0).ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(detalle.Subtotal.ToString("N2")).FontSize(9);
                            }
                        });

                        // Cuadro de Totales (Grises)
                        col.Item().AlignRight().Width(250).Column(c =>
                        {
                            c.Item().PaddingVertical(5).Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                row.ConstantItem(100).AlignRight().Text($"{cotizacion.Subtotal:N2} {cotizacion.Moneda}").FontSize(10);
                            });

                            c.Item().PaddingVertical(2).Row(row =>
                            {
                                row.RelativeItem().Text("Descuento:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                row.ConstantItem(100).AlignRight().Text($"{cotizacion.Descuento:N2} {cotizacion.Moneda}").FontSize(10);
                            });

                            c.Item().PaddingVertical(2).Row(row =>
                            {
                                row.RelativeItem().Text("Impuesto:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                row.ConstantItem(100).AlignRight().Text($"{cotizacion.Impuesto:N2} {cotizacion.Moneda}").FontSize(10);
                            });

                            // Total destacado (Gris oscuro)
                            c.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").FontSize(12).Bold();
                                row.ConstantItem(100).AlignRight().Text($"{cotizacion.Total:N2} {cotizacion.Moneda}")
                                    .FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                            });
                        });
                    });

                    // --- FOOTER ---
                    page.Footer().AlignCenter().Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        c.Item().PaddingTop(5).Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }
    }
}
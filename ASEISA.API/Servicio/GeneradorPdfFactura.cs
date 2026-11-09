using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Servicio
{
    public class GeneradorPdfFactura : IGeneradorPdfFactura
    {
        public byte[] Generar(FacturaDetalle factura, Empresa? empresa)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(empresa?.NombreComercial ?? "ASEISA")
                                    .FontSize(24).Bold().FontColor("#ff4831");
                                c.Item().Text(empresa?.RazonSocial ?? "").FontSize(9);
                                c.Item().Text($"Cédula: {empresa?.CedulaJuridica ?? "-"}").FontSize(9);
                                c.Item().Text($"Tel: {empresa?.Telefono ?? "-"}").FontSize(9);
                                c.Item().Text(empresa?.Direccion ?? "").FontSize(9);
                            });

                            row.ConstantItem(160).Column(c =>
                            {
                                c.Item().AlignRight().Text("FACTURA").FontSize(16).Bold().FontColor(Colors.Grey.Darken2);
                                c.Item().AlignRight().Text($"#{factura.NumeroFactura}").FontSize(14).Bold();
                                c.Item().AlignRight().Text($"Fecha: {factura.FechaEmision:dd/MM/yyyy}").FontSize(9);
                                if (factura.FechaVencimiento.HasValue)
                                    c.Item().AlignRight().Text($"Vence: {factura.FechaVencimiento:dd/MM/yyyy}").FontSize(9);
                                c.Item().AlignRight().Text($"Estado: {factura.Estado}").FontSize(9);
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
                            c.Item().Text(factura.NombreCliente ?? "").FontSize(13).Bold();
                            c.Item().Text($"Identificación: {factura.Identificacion ?? "-"}");
                            if (!string.IsNullOrEmpty(factura.CorreoCliente))
                                c.Item().Text($"Correo: {factura.CorreoCliente}");
                            if (!string.IsNullOrEmpty(factura.TerminoPagoNombre))
                                c.Item().Text($"Término de pago: {factura.TerminoPagoNombre}");
                            if (!string.IsNullOrEmpty(factura.CondicionVenta))
                                c.Item().Text($"Condición: {factura.CondicionVenta}");
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(70);
                            });

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

                            foreach (var producto in factura.Productos)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(producto.Codigo ?? "-").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(producto.Nombre ?? "-").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(producto.Cantidad.ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(producto.PrecioUnitario.ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text((producto.PorcentajeDescuento ?? 0).ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text((producto.PorcentajeImpuesto ?? 0).ToString("N2")).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(producto.Subtotal.ToString("N2")).FontSize(9);
                            }
                        });

                        col.Item().AlignRight().Width(250).Column(c =>
                        {
                            var moneda = factura.Moneda ?? empresa?.MonedaBase ?? "";

                            c.Item().PaddingVertical(5).Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                row.ConstantItem(100).AlignRight().Text($"{factura.Subtotal:N2} {moneda}").FontSize(10);
                            });

                            c.Item().PaddingVertical(2).Row(row =>
                            {
                                row.RelativeItem().Text("Descuento:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                row.ConstantItem(100).AlignRight().Text($"{factura.Descuento:N2} {moneda}").FontSize(10);
                            });

                            c.Item().PaddingVertical(2).Row(row =>
                            {
                                row.RelativeItem().Text("Impuesto:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                row.ConstantItem(100).AlignRight().Text($"{factura.Impuesto:N2} {moneda}").FontSize(10);
                            });

                            c.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").FontSize(12).Bold();
                                row.ConstantItem(100).AlignRight().Text($"{factura.Total:N2} {moneda}")
                                    .FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                            });
                        });
                    });

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

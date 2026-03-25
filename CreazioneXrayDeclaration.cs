using GestioneSicurezze.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Reflection;



namespace GestioneSicurezze
{    
    public class CreazioneXrayDeclaration : IDocument
    {
        private readonly ModelloXray modelloXray;
        string backGroundColor = "#F2F2F2";
        string verdana = "Verdana";
        string ImageDir = AppDomain.CurrentDomain.BaseDirectory;
        public CreazioneXrayDeclaration(ModelloXray modelloXray)
        {
            this.modelloXray = modelloXray;
        }
        
        // Funzione helper (chiamala una volta)
        public static byte[] GetImageBytes(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource {resourceName} not found");
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        public void Compose(IDocumentContainer container)
        {
            container
            .Page(page =>
            {
                page.MarginHorizontal(.5f, Unit.Centimetre);
                page.MarginVertical(.5f, Unit.Centimetre);
                page.Size(pageSize: PageSizes.A4);

                page.Header().AlignCenter().Element(ComposeHeader);
                page.Content().PaddingTop(.5f, Unit.Millimetre).Element(ComposeContent);
                page.Footer().AlignCenter().Element(ComposeFooter);
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.Column(c =>
            {
                c.Item().Row(r =>
                {
                    if(modelloXray.CodiceEnac == "IT/RA/00217-03")
                    {
                        var imageBytes = GetImageBytes("GestioneSicurezze.Immagini.IndirizzoVignateImage.png");
                        r.RelativeItem().Image(imageBytes);
                        //r.RelativeItem().Image(Path.Combine(ImageDir, "Immagini", "IndirizzoVignateImage.png"));
                    }
                    else if(modelloXray.CodiceEnac == "IT/RA/00217-02")
                    {
                        var imageBytes = GetImageBytes("GestioneSicurezze.Immagini.IndirizzoLiscateImage.png");
                        r.RelativeItem().Image(imageBytes);
                        //r.RelativeItem().Image(Path.Combine(ImageDir,"Immagini","IndirizzoLiscateImage.png"));
                    }
                    else
                    {
                        var imageBytes = GetImageBytes("GestioneSicurezze.Immagini.IndirizzoVignateOldImage.png");
                        r.RelativeItem().Image(imageBytes);
                        //r.RelativeItem().Image(Path.Combine(ImageDir, "Immagini", "IndirizzoVignateOldImage.png"));
                    }
                });

                c.Item().Row(r =>
                {
                    var imageBytes = GetImageBytes("GestioneSicurezze.Immagini.BottomImage.png");
                    r.RelativeItem().Image(imageBytes);
                    /*r.RelativeItem()
                        .Image(Path.Combine(ImageDir,"Immagini","BottomImage.png"));*/
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(c =>
            {
                c.Item().AlignCenter().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().AlignMiddle().Text("Pr").FontSize(16).FontFamily("Calibri").Bold();
                    r.AutoItem().AlignMiddle().PaddingLeft(10).Background(Color.FromHex("#F2F2F2")).Text(modelloXray.Progressivo.ToString()).FontSize(16).FontFamily("Calibri").Bold();                    
                    r.AutoItem().AlignMiddle().PaddingLeft(10).AlignCenter().Text("Codice alfanumerico database EU:").FontSize(16).FontFamily(verdana);
                    r.AutoItem().AlignMiddle().PaddingLeft(10).Background(Color.FromHex(backGroundColor)).Text(modelloXray.CodiceEnac)
                    .FontSize(20).FontFamily("Calibri").Bold().Underline();
                });

                c.Item().AlignCenter().PaddingTop(3, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().AlignMiddle().Text("Certifica che le sotto elencate spedizioni a noi affidate da:").FontSize(10)
                            .FontFamily(verdana);
                    r.AutoItem().AlignMiddle().PaddingLeft(5).Text(modelloXray.Cliente).FontFamily("Calibri").Bold().FontSize(18);
                });

                c.Item().AlignCenter().PaddingTop(3, Unit.Millimetre).Table(table =>
                {
                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });

                    //Testata Tabella
                    table.Cell().Element(CellStyle).Text("AWB").AlignCenter().Bold().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyle).Text("COLLI").AlignCenter().Bold().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyle).Text("KG").AlignCenter().Bold().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyle).Text("DEST").AlignCenter().Bold().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyle).Text("Contenuto").AlignCenter().Bold().FontSize(11).FontFamily(verdana);

                    //Contenuto Tabella
                    table.Cell().Element(CellStyleContent).Text(modelloXray.Awb).AlignCenter().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyleContent).Text(modelloXray.Colli).AlignCenter().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyleContent).Text(modelloXray.Peso).AlignCenter().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyleContent).Text(modelloXray.Destinazione).AlignCenter().FontSize(11).FontFamily(verdana);
                    table.Cell().Element(CellStyleContent).Text(modelloXray.Contenuto).AlignCenter().FontSize(11).FontFamily(verdana);

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Border(1).Padding(5);
                    }

                    static IContainer CellStyleContent(IContainer container)
                    {
                        return container.Border(1).Padding(10);
                    }
                });

                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().Text("Sono state protette contro ogni interferenza illecita  dalla consegna fino all'imbarco").
                        FontSize(10).FontFamily(verdana).Bold();
                });

                c.Item().AlignLeft().Row(r =>
                {
                    r.AutoItem().Text("delle stesse sui voli / aviocamionati in partenza da questo aeroporto:").
                        FontSize(10).FontFamily(verdana).Bold();
                });

                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().Text("RIFERIMENTO").
                        FontSize(11).FontFamily(verdana);
                    r.RelativeItem().BorderBottom(1).PaddingLeft(3).Text(modelloXray.Riferimento)
                        .FontSize(11).FontFamily(verdana);
                });

                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().Text("Trasportatore/Rag Soc").
                        FontSize(11).FontFamily(verdana);
                    r.RelativeItem().BorderBottom(1).PaddingLeft(5).Text(modelloXray.Trasportatore)
                        .FontSize(11).FontFamily(verdana);
                });

                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().Text("Targhe").
                        FontSize(11).FontFamily(verdana);
                    r.ConstantItem(100).BorderBottom(1).PaddingLeft(10).Text(modelloXray.Targa)
                        .FontSize(11).FontFamily(verdana);
                    r.AutoItem().PaddingLeft(25).Text("SIGILLO NUMERO")
                        .FontSize(11).FontFamily(verdana).Bold().Underline();
                    r.RelativeItem().BorderBottom(1).PaddingLeft(10).Text(modelloXray.SigilloNumero)
                        .FontSize(11).FontFamily(verdana);
                });

                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().Text("Nome Autista").
                        FontSize(11).FontFamily(verdana);
                    r.RelativeItem().BorderBottom(1).PaddingLeft(10).Text(modelloXray.Autista)
                        .FontSize(11).FontFamily(verdana);
                });

                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.RelativeItem().Table(table =>
                    {
                        table.ColumnsDefinition(col =>
                        {
                            col.ConstantColumn(120, Unit.Point);
                            col.RelativeColumn();
                        });

                        string xray = "";
                        if (modelloXray.XRAY)
                        {
                            xray = "X";
                        }
                        string etd = "";
                        if (modelloXray.ETD)
                        {
                            etd = "X";
                        }
                        string phs = "";
                        if (modelloXray.PHS)
                        {
                            phs = "X";
                        }
                        string vck = "";
                        if (modelloXray.VCK)
                        {
                            vck = "X";
                        }
                        table.Cell().Element(CellStyle).Text(xray).Bold().FontSize(16).FontFamily("Calibri");
                        table.Cell().AlignMiddle().AlignLeft().PaddingLeft(20).Text("XRAY").Bold().FontSize(14).FontFamily(verdana);
                        table.Cell().Element(CellStyle).Text(etd).Bold().FontSize(16).FontFamily("Calibri");
                        table.Cell().AlignMiddle().AlignLeft().PaddingLeft(20).Text("ETD").Bold().FontSize(14).FontFamily(verdana);
                        table.Cell().Element(CellStyle).Text(phs).Bold().FontSize(16).FontFamily("Calibri");
                        table.Cell().AlignMiddle().AlignLeft().PaddingLeft(20).Text("PHS (ispezione manuale)").Bold().FontSize(14).FontFamily(verdana);
                        table.Cell().Element(CellStyle).Text(vck).Bold().FontSize(16).FontFamily("Calibri");
                        table.Cell().AlignMiddle().AlignLeft().PaddingLeft(20).Text("VCK (ispezione visiva completa)").Bold().FontSize(14).FontFamily(verdana);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.Border(1).Padding(3).AlignMiddle().AlignCenter();
                        }
                    });
                });

                c.Item().AlignCenter().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.RelativeItem().AlignCenter().Text("STATO DELLA MERCE").FontSize(16).FontFamily(verdana).Bold();
                });

                c.Item().AlignLeft().PaddingTop(3, Unit.Millimetre).Row(r =>
                {
                    r.RelativeItem().Table(table =>
                    {
                        table.ColumnsDefinition(col =>
                        {
                            col.ConstantColumn(120, Unit.Point);
                            col.RelativeColumn();
                        });

                        if (modelloXray.STATOMERCE == "SPX")
                        {
                            table.Cell().Element(CellStyle).Text(modelloXray.STATOMERCE).Bold().FontSize(16).FontFamily("Calibri");
                        }
                        else
                        {
                            table.Cell().Element(CellStyle).Text("").Bold().FontSize(16).FontFamily("Calibri");
                        }
                        table.Cell().PaddingLeft(20).Row(r =>
                        {
                            r.AutoItem().AlignMiddle().Text("SPX").Bold().FontSize(14).FontFamily(verdana);
                            r.RelativeItem().AlignMiddle().PaddingLeft(2)
                            .Text("(sicura per ogni tipo di aeromobile)").FontFamily(verdana).FontSize(11);
                        });

                        if (modelloXray.STATOMERCE == "SHR")
                        {
                            table.Cell().Element(CellStyle).Text(modelloXray.STATOMERCE).Bold().FontSize(16).FontFamily("Calibri");
                        }
                        else
                        {
                            table.Cell().Element(CellStyle).Text("").Bold().FontSize(16).FontFamily("Calibri");
                        }
                        table.Cell().PaddingLeft(20).Row(r =>
                        {
                            r.AutoItem().AlignMiddle().Text("SHR").Bold().FontSize(14).FontFamily(verdana);
                            r.RelativeItem().AlignMiddle().PaddingLeft(2)
                            .Text("(sicura solo per trasporto con aeromobili “all cargo” o postali)").FontFamily(verdana).FontSize(11);
                        });

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.Border(1).Padding(3).AlignMiddle().AlignCenter();
                        }
                    });
                });
                 
                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.RelativeItem().Table(table =>
                    {
                        table.ColumnsDefinition(col =>
                        {
                            col.RelativeColumn();
                            col.RelativeColumn();
                        });

                        table.Cell().Element(CellStyle).Text("Data e ora rilascio status di sicurezza").Bold().FontSize(11).FontFamily(verdana);
                        table.Cell().Element(CellStyleTime).Text(modelloXray.DataEsecuzione.ToString()).Bold().FontSize(11).FontFamily(verdana);

                    });

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).AlignMiddle().AlignCenter().Padding(5);
                    }

                    static IContainer CellStyleTime(IContainer container)
                    {
                        return container.Border(1).AlignMiddle().AlignCenter().Padding(5);
                    }
                });

                c.Item().AlignLeft().PaddingTop(2, Unit.Millimetre).Row(r =>
                {
                    r.AutoItem().Column(c =>
                    {
                        c.Item().AlignMiddle().Text("OPERATORE").FontFamily("Calibri").Bold().FontSize(14);
                    });

                    r.ConstantItem(100).PaddingLeft(15).Column(c =>
                    {
                        c.Item().AlignCenter().Border(1).Text(modelloXray.Operatore).FontFamily("Calibri").Bold().FontSize(36);
                    });

                    r.RelativeItem().PaddingLeft(30).BorderBottom(1).Column(c =>
                    {
                        c.Item().Text("Firma e timbro").FontFamily(verdana).FontSize(11);
                    });
                });

                c.Item().AlignLeft().PaddingTop(5, Unit.Millimetre).PaddingLeft(110).Row(r =>
                {
                    r.AutoItem().Column(c =>
                    {
                        c.Item().AlignMiddle().Text("DESTINAZIONE").FontFamily("Calibri").Bold().FontSize(14).Underline();
                    });

                    r.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingLeft(3).BorderBottom(1).AlignMiddle().Text(modelloXray.AeroportoDest).FontFamily("Arial").Bold().FontSize(14);
                    });
                });
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    var imageBytes = GetImageBytes("GestioneSicurezze.Immagini.TopImagePdf.jpg");
                    column.Item()
                        .Image(imageBytes);
                    /*column.Item()
                        .Image(Path.Combine(ImageDir,"Immagini","TopImagePdf.jpg"));*/
                    column.Item().PaddingTop(2, Unit.Millimetre)
                        .Text("DICHIARAZIONE DI SICUREZZA DELLA SPEDIZIONE DI MERCE")
                        .FontSize(12).FontFamily(verdana).AlignCenter();
                    column.Item().AlignCenter().AlignMiddle().PaddingHorizontal(15).PaddingTop(2, Unit.Millimetre).Row(r =>
                    {
                        r.AutoItem()
                        .Text("Agente Regolamentato :").FontSize(11).FontFamily(verdana);
                        r.RelativeItem().PaddingLeft(10)
                        .Text("AUTOTRASPORTI MEZZAPESA MASSIMILIANO SRL").FontSize(12)
                        .BackgroundColor(Color.FromHex(backGroundColor))
                        .FontFamily(verdana).Bold();
                    });
                });
            });
        }
    }
}
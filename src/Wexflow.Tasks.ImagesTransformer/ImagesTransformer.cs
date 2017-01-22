using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Wexflow.Tasks.ImagesTransformer
{
    public enum ImgFormat
    {
        Bmp,
        Emf,
        Exif,
        Gif,
        Icon,
        Jpeg,
        Png,
        Tiff,
        Wmf
    }

    public class ImagesTransformer:Task
    {
        public string OutputFilePattern { get; private set; }
        public ImgFormat OutputFormat { get; private set; }

        public ImagesTransformer(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.OutputFilePattern = this.GetSetting("outputFilePattern");
            this.OutputFormat = (ImgFormat)Enum.Parse(typeof(ImgFormat), this.GetSetting("outputFormat"), true);
        }

        public override void Run()
        {
            this.Info("Transforming images...");
           
            foreach (FileInf file in this.SelectFiles())
            {
                try
                {
                    string destFilePath = Path.Combine(this.Workflow.WorkflowTempFolder,
                        this.OutputFilePattern.Replace("$fileNameWithoutExtension", Path.GetFileNameWithoutExtension(file.FileName)).Replace("$fileName", file.FileName));

                    using (Image img = Image.FromFile(file.Path))
                    {
                        switch (this.OutputFormat)
                        {
                            case ImgFormat.Bmp:
                                img.Save(destFilePath, ImageFormat.Bmp);
                                break;
                            case ImgFormat.Emf:
                                img.Save(destFilePath, ImageFormat.Emf);
                                break;
                            case ImgFormat.Exif:
                                img.Save(destFilePath, ImageFormat.Exif);
                                break;
                            case ImgFormat.Gif:
                                img.Save(destFilePath, ImageFormat.Gif);
                                break;
                            case ImgFormat.Icon:
                                img.Save(destFilePath, ImageFormat.Icon);
                                break;
                            case ImgFormat.Jpeg:
                                img.Save(destFilePath, ImageFormat.Jpeg);
                                break;
                            case ImgFormat.Png:
                                img.Save(destFilePath, ImageFormat.Png);
                                break;
                            case ImgFormat.Tiff:
                                img.Save(destFilePath, ImageFormat.Tiff);
                                break;
                            case ImgFormat.Wmf:
                                img.Save(destFilePath, ImageFormat.Wmf);
                                break;
                        }
                    }
                    this.Files.Add(new FileInf(destFilePath, this.Id));
                    this.InfoFormat("Image {0} transformed to {1}", file.Path, destFilePath);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while transforming the image {0}. Error: {1}", file.Path, e.Message);
                }
            }
            
            this.Info("Task finished.");
        }
    }
}

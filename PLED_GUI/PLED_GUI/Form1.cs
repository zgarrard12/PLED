﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IngestImage;
using System.IO;

namespace PLED_GUI
{
    public partial class Form1 : Form
    {

        //defines return values to be used for wood dimension window
        public string ReturnValue1 { get; set; }
        public string ReturnValue2 { get; set; }

        //defines strings, ints, and doubles to be used
        string xDimension, yDimension, imgPathString, filePath, outFilePath;
        int xDimPix, yDimPix, xLocPix, yLocPix, xImgSize, yImgSize;
        double xyratio, xywoodratio, xdoubletmp, ydoubletmp, tmpwoodsize, time;

        public Form1()
        {
            //Initializes application
            InitializeComponent();

            //initializes file dialog settings
            openFileDialog1.Title = "Load PLED Image";
            openFileDialog1.InitialDirectory = @"C:";
            openFileDialog1.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg; *.jpeg; *.png; *.gif; *.bmp";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            //initialize save dialog settings
            saveFileDialog1.Title = "Select output File";
            saveFileDialog1.InitialDirectory = filePath;
            saveFileDialog1.Filter = "Output Filename (no extension)|";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            //start image load button disabled
            imgLoad.Enabled = false;
        }

        //handler for clicking on SelectWoodDimensions button
        private void SelectWoodDimensions_Click(object sender, EventArgs e)
        {
            //creates instance of window
            using (var getWoodDimensions = new woodDimensions())
            {
                //opens window
                var result = getWoodDimensions.ShowDialog();

                //set invalid or closed case
                if (result == DialogResult.Abort)
                {
                    if(getWoodDimensions.ReturnValue1 != null)
                    {
                        x_woodDimension.Text = getWoodDimensions.ReturnValue1;
                        y_woodDimension.Text = getWoodDimensions.ReturnValue2;
                    }
                }

                if (result == DialogResult.OK)
                {
                    //saves dimensions in inches
                    xDimension = getWoodDimensions.ReturnValue1;
                    yDimension = getWoodDimensions.ReturnValue2;

                    //displays dimensions in inches
                    x_woodDimension.Text = xDimension;
                    y_woodDimension.Text = yDimension;

                    //converts, saves and displays dimensions in pixel count
                    xDimPix = Convert.ToUInt16(Convert.ToDouble(xDimension) / 0.006);   //ORIGINAL 0.009
                    yDimPix = Convert.ToUInt16(Convert.ToDouble(yDimension) / 0.006);   //ORIGINAL 0.009
                    xWoodDimPixels.Text = Convert.ToString(xDimPix);
                    yWoodDimPixels.Text = Convert.ToString(yDimPix);

                    //sets max slider value based on dimension in pixels
                    xSlider.Maximum = xDimPix - xImgSize;
                    ySlider.Maximum = yDimPix - yImgSize;

                    //set wood plaque ratio and dimensions
                    xywoodratio = Convert.ToDouble(xDimPix) / Convert.ToDouble(yDimPix);
                    if (xDimPix > yDimPix)
                    {
                        PlaqueSize.Width = 330;
                        tmpwoodsize = 330 * (1 / xywoodratio);
                        PlaqueSize.Height = Convert.ToUInt16(tmpwoodsize);
                    }
                    else if (xDimPix < yDimPix)
                    {
                        PlaqueSize.Height = 330;
                        tmpwoodsize = 330 * xywoodratio;
                        PlaqueSize.Width = Convert.ToUInt16(tmpwoodsize);
                    }
                    else
                    {
                        PlaqueSize.Width = PlaqueSize.Height = 330;
                    }

                    if(xImgSize != 0 && yImgSize !=0)
                    {
                        while (xImgSize > xDimPix || yImgSize > yDimPix)
                        {
                            if (xImgSize > yImgSize)
                            {
                                //adjust image size (pixel count) according to plaque size if image is larger
                                if (xImgSize > xDimPix)
                                {
                                    //sets y image dimension to y wood dimension
                                    xImgSize = xDimPix;
                                    //sets x dimension according to conversion above
                                    ydoubletmp = xImgSize * (1 / xyratio);
                                    yImgSize = Convert.ToUInt16(ydoubletmp);
                                }
                                //adjust image size (pixel count) according to plaque size if image is larger
                                else
                                {
                                    //sets x image dimension to x wood dimension
                                    yImgSize = yDimPix;
                                    //sets y dimension according to conversion above
                                    xdoubletmp = yImgSize * xyratio;
                                    xImgSize = Convert.ToUInt16(xdoubletmp);
                                }
                            }

                            else if (yImgSize > xImgSize)
                            {
                                //adjust image size (pixel count) according to plaque size if image is larger
                                if (yImgSize > yDimPix)
                                {
                                    //sets x image dimension to x wood dimension
                                    yImgSize = yDimPix;
                                    //sets y dimension according to conversion above
                                    xdoubletmp = yImgSize * xyratio;
                                    xImgSize = Convert.ToUInt16(xdoubletmp);
                                }
                                //adjust image size (pixel count) according to plaque size if image is larger
                                else
                                {
                                    //sets y image dimension to y wood dimension
                                    xImgSize = xDimPix;
                                    //sets x dimension according to conversion above
                                    ydoubletmp = xImgSize * (1 / xyratio);
                                    yImgSize = Convert.ToUInt16(ydoubletmp);
                                }
                            }

                        }

                        //set max slider location positions
                        xSlider.Maximum = xDimPix - xImgSize;
                        ySlider.Maximum = yDimPix - yImgSize;

                        //set max slider sizes
                        ximgslider.Maximum = xImgSize;
                        yimgslider.Maximum = yImgSize;

                        //set default position and size values
                        xSlider.Value = 0;
                        ySlider.Value = 0;
                        ximgslider.Value = xImgSize;
                        yimgslider.Value = yImgSize;
                        ximgsizebox.Text = Convert.ToString(xImgSize);
                        yimgsizebox.Text = Convert.ToString(yImgSize);
                        xLocBox.Text = "0";
                        yLocBox.Text = "0";

                        //set image size
                        xdoubletmp = (xImgSize * PlaqueSize.Width) / xDimPix;
                        ydoubletmp = xdoubletmp * (1 / xyratio);
                        imgBox.Width = Convert.ToUInt16(xdoubletmp);
                        imgBox.Height = Convert.ToUInt16(ydoubletmp);
                    }
                    //disable button
                    SelectWoodDimensions.Enabled = false;
                    imgLoad.Enabled = true;
                }
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
           
        }

        //handler for imgLoad button
        private void imgLoad_Click(object sender, EventArgs e)
        {
            //opens openFileDialog1 form
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                //obtains file path
                imgPathString = openFileDialog1.FileName;
                filePath = Path.GetDirectoryName(imgPathString);
                imgPath.Text = imgPathString;
                //loads image
                Image inputImage = Image.FromFile(imgPathString);
                //c is the image in bitmap
                Bitmap c = new Bitmap(inputImage);

                //sets image size according to image
                xImgSize = c.Width;
                yImgSize = c.Height;
                
                //sets xyratio
                xyratio = Convert.ToDouble(xImgSize) / Convert.ToDouble(yImgSize);

                while (xImgSize > xDimPix || yImgSize > yDimPix)
                {
                    if (xImgSize == yImgSize)
                    {
                        //adjust image size (pixel count) to fit plaque
                        xImgSize = xDimPix;
                        yImgSize = yDimPix;
                    }

                    else if (xImgSize > yImgSize)
                    {
                        //adjust image size (pixel count) according to plaque size if image is larger
                        if (xImgSize > xDimPix)
                        {
                            //sets y image dimension to y wood dimension
                            xImgSize = xDimPix;
                            //sets x dimension according to conversion above
                            ydoubletmp = xImgSize * (1 / xyratio);
                            yImgSize = Convert.ToUInt16(ydoubletmp);
                        }
                    }

                    else if (yImgSize > xImgSize)
                    {
                        //adjust image size (pixel count) according to plaque size if image is larger
                        if (yImgSize > yDimPix)
                        {
                            //sets x image dimension to x wood dimension
                            yImgSize = yDimPix;
                            //sets y dimension according to conversion above
                            xdoubletmp = yImgSize * xyratio;
                            xImgSize = Convert.ToUInt16(xdoubletmp);
                        }
                    }
                   
                }

                //set max slider location positions
                xSlider.Maximum = xDimPix - xImgSize;
                ySlider.Maximum = yDimPix - yImgSize;

                //set max slider sizes
                ximgslider.Maximum = xImgSize;
                yimgslider.Maximum = yImgSize;

                //set default position and size values
                xSlider.Value = 0;
                ySlider.Value = 0;
                ximgslider.Value = xImgSize;
                yimgslider.Value = yImgSize;
                ximgsizebox.Text = Convert.ToString(xImgSize);
                yimgsizebox.Text = Convert.ToString(yImgSize);
                xLocBox.Text = "0";
                yLocBox.Text = "0";

                //set image size
                xdoubletmp = (xImgSize * PlaqueSize.Width) / xDimPix;
                ydoubletmp = xdoubletmp * (1 / xyratio);
                imgBox.Width = Convert.ToUInt16(xdoubletmp);
                imgBox.Height = Convert.ToUInt16(ydoubletmp);
            }
            imgBox.ImageLocation = imgPathString;
            imgLoad.Enabled = false;

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        //x slider handler
        private void xSlider_Scroll(object sender, EventArgs e)
        {
            //allows user to adjust location to place image on x axis
            xLocPix = xSlider.Value;
            //displays pixel location
            xLocBox.Text = Convert.ToString(xLocPix);

            //moves image block representation on x axis
            xdoubletmp = ((Convert.ToDouble(xLocPix) * (PlaqueSize.Width - Convert.ToDouble(imgBox.Width))) / Convert.ToDouble(xSlider.Maximum));
            imgBox.Location = new Point((440 + Convert.ToUInt16(xdoubletmp)), imgBox.Location.Y);

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        //y slider handler
        private void ySlider_Scroll(object sender, EventArgs e)
        {
            //allows user to adjust location to place image on y axis
            yLocPix = ySlider.Value;
            //displays pixel location
            yLocBox.Text = Convert.ToString(yLocPix);

            //moves image block representation on y axis
            ydoubletmp = ((Convert.ToDouble(yLocPix) * (PlaqueSize.Height - Convert.ToDouble(imgBox.Height))) / Convert.ToDouble(ySlider.Maximum));
            imgBox.Location = new Point(imgBox.Location.X, (13 + Convert.ToUInt16(ydoubletmp)));

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        //centers image
        private void cent_Click(object sender, EventArgs e)
        {
            //centers on actual image
            xLocPix = (xDimPix - xImgSize) / 2;
            yLocPix = (yDimPix - yImgSize) / 2;
            xSlider.Value = xLocPix;
            ySlider.Value = yLocPix;
            xLocBox.Text = Convert.ToString(xLocPix);
            yLocBox.Text = Convert.ToString(yLocPix);

            //centers on diagram
            if(xDimPix != xImgSize)
            {
                xdoubletmp = ((Convert.ToDouble(xLocPix) * (PlaqueSize.Width - Convert.ToDouble(imgBox.Width))) / Convert.ToDouble(xSlider.Maximum));
                imgBox.Location = new Point((440 + Convert.ToUInt16(xdoubletmp)), imgBox.Location.Y);
            }
            
            if(yDimPix != yImgSize)
            {
                ydoubletmp = ((Convert.ToDouble(yLocPix) * (PlaqueSize.Height - Convert.ToDouble(imgBox.Height))) / Convert.ToDouble(ySlider.Maximum));
                imgBox.Location = new Point(imgBox.Location.X, (13 + Convert.ToUInt16(ydoubletmp)));
            }

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        //vertically centers image
        private void vertCent_Click(object sender, EventArgs e)
        {
            //vertically centers actual image
            yLocPix = (yDimPix - yImgSize) / 2;
            ySlider.Value = yLocPix;
            yLocBox.Text = Convert.ToString(yLocPix);

            //vertically centers diagram
            if (yDimPix != yImgSize)
            {
                ydoubletmp = ((Convert.ToDouble(yLocPix) * (PlaqueSize.Height - Convert.ToDouble(imgBox.Height))) / Convert.ToDouble(ySlider.Maximum));
                imgBox.Location = new Point(imgBox.Location.X, (13 + Convert.ToUInt16(ydoubletmp)));
            }

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        //horizontally centers image
        private void horzCent_Click(object sender, EventArgs e)
        {
            //horizontally centers actual image
            xLocPix = (xDimPix - xImgSize) / 2;
            xSlider.Value = xLocPix;
            xLocBox.Text = Convert.ToString(xLocPix);

            //horizontally centers diagram
            if(xDimPix != xImgSize)
            {
                xdoubletmp = ((Convert.ToDouble(xLocPix) * (PlaqueSize.Width - Convert.ToDouble(imgBox.Width))) / Convert.ToDouble(xSlider.Maximum));
                imgBox.Location = new Point((440 + Convert.ToUInt16(xdoubletmp)), imgBox.Location.Y);
            }

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        //handler for ximgslider slider
        private void ximgslider_Scroll(object sender, EventArgs e)
        {
            //allows user to adjust image size on x axis
            xImgSize = ximgslider.Value;
            //displays pixel count
            ximgsizebox.Text = Convert.ToString(xImgSize);

            //sets y size according to conversion above and updates slider
            ydoubletmp = xImgSize * (1 / xyratio);
            yImgSize = Convert.ToUInt16(ydoubletmp);
            yimgsizebox.Text = Convert.ToString(yImgSize);
            yimgslider.Value = yImgSize;

            //set max slider location positions
            xSlider.Maximum = xDimPix - xImgSize;
            ySlider.Maximum = yDimPix - yImgSize;

            //set default slider location
            xSlider.Value = 0;
            ySlider.Value = 0;
            xLocPix = 0;
            yLocPix = 0;
            xLocBox.Text = "0";
            yLocBox.Text = "0";
            imgBox.Location = new Point(440, 13);

            //set image size
            xdoubletmp = (xImgSize * PlaqueSize.Width) / xDimPix;
            ydoubletmp = xdoubletmp * (1 / xyratio);
            imgBox.Width = Convert.ToUInt16(xdoubletmp);
            imgBox.Height = Convert.ToUInt16(ydoubletmp);

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        //handler for yimgslider slider
        private void yimgslider_Scroll(object sender, EventArgs e)
        {
            //allows user to adjust image size on x axis
            yImgSize = yimgslider.Value;
            //displays pixel count
            yimgsizebox.Text = Convert.ToString(yImgSize);

            //sets y size according to conversion above and updates slider
            xdoubletmp = yImgSize * xyratio;
            xImgSize = Convert.ToUInt16(xdoubletmp);
            ximgsizebox.Text = Convert.ToString(xImgSize);
            ximgslider.Value = xImgSize;

            //set max slider location positions
            xSlider.Maximum = xDimPix - xImgSize;
            ySlider.Maximum = yDimPix - yImgSize;

            //set default slider location
            xSlider.Value = 0;
            ySlider.Value = 0;
            xLocPix = 0;
            yLocPix = 0;
            xLocBox.Text = "0";
            yLocBox.Text = "0";
            imgBox.Location = new Point(440, 13);

            //set image size
            xdoubletmp = (xImgSize * PlaqueSize.Width) / xDimPix;
            ydoubletmp = xdoubletmp * (1 / xyratio);
            imgBox.Width = Convert.ToUInt16(xdoubletmp);
            imgBox.Height = Convert.ToUInt16(ydoubletmp);

            //Estimates engrave time for image
            time = (((Convert.ToDouble(xImgSize) * Convert.ToDouble(yImgSize) * (0.0155)) + (0.0005 * (Convert.ToDouble(xLocPix) + Convert.ToDouble(yLocPix)))) / 3600);
            EngTime.Text = Convert.ToString(time);
        }

        private void submitPLED_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog();
            if(result == DialogResult.OK)
            {
                //select save path
                outFilePath = saveFileDialog1.FileName;

                //pass everything to image ingestion backend
                IngestImage.Program imageIngest = new IngestImage.Program();
                imageIngest.Ingest(xImgSize, yImgSize, xLocPix, yLocPix, imgPathString, outFilePath);

                //create outpath file
                StreamWriter sw = File.CreateText(@"C:\PLED\PLEDpath.txt");
                sw.Write(outFilePath + ".gcode");
                sw.Close();
                               
                //tell user job ready to begin begins the engraving process
                var jobComplete = new complete();
                jobComplete.ShowDialog();

                var eng = new engraving();

                Application.Exit();
            }
        }

        //handler for endPLED button
        private void endPLED_Click_1(object sender, EventArgs e)
        {
            //closes application
            Application.Exit();
        }

    }
}

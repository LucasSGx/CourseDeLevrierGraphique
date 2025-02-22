﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CourseDeLevrierGraphique
{
    public partial class CourseForm : Form
    {
        private PictureBox[] levriers;
        private const int DistanceCourse = 1000;
        private Random random = new Random();
        private int[] distancesParcourues;
        private Thread[] threads;
        private bool[] courseTerminee;
        private Mutex mutex = new Mutex();
        private int nombreLevrier;
        private int placesFinies = 0;

        private ManualResetEvent depart;

        private ManualResetEvent[] arriveesDrapeaux;
        private int[] resultat;

        public CourseForm(int nombreLevrier)
        {
            InitializeComponent();
            this.nombreLevrier = nombreLevrier;
            InitialiserCourse(nombreLevrier);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void InitialiserCourse(int nombreLevrier)
        {
            this.Text = "Course BTS CIEL";
            this.ClientSize = new Size(1500, nombreLevrier * 60 + 50);

            levriers = new PictureBox[nombreLevrier];
            distancesParcourues = new int[nombreLevrier];
            threads = new Thread[nombreLevrier];
            courseTerminee = new bool[nombreLevrier];

            string[] imagePaths = new string[]
            {
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Lucas.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Joel.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Sorin.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Oceane.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Pavlo.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Arthur.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Baptiste.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Evan.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Etini.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Galen.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Hugo.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Mael.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Nicolas.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Ptit_Robin.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Souleyman.png",
                "C:\\Users\\lsangeroteo\\Desktop\\CourseDeLevrierGraphique\\CourseDeLevrierGraphique\\ImagesLevrier\\Corentin.png",

            };

            for (int i = 0; i < nombreLevrier; i++)
            {
                levriers[i] = new PictureBox
                {
                    Size = new Size(50, 50),
                    Location = new Point(10, 50 + (i * 60)),
                    BackColor = Color.Transparent,
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                try
                {
                    levriers[i].Image = Image.FromFile(imagePaths[i % imagePaths.Length]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement de l'image : {ex.Message}",
                                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.Controls.Add(levriers[i]);
            }
            depart = new ManualResetEvent(false);
            arriveesDrapeaux = new ManualResetEvent[nombreLevrier];
            resultat = new int[nombreLevrier];
            for (int i = 0; i < nombreLevrier; i++)
            {
                int index = i;
                arriveesDrapeaux[i] = new ManualResetEvent(false);
                threads[i] = new Thread(() => DeplacerLevrier(index));
                threads[i].Start();
            }

            Thread arrriveesCoureur = new Thread(this.gestionArrivee);
            arrriveesCoureur.Start();


            depart.Set();
        }

        private void gestionArrivee(object? obj)
        {
            for(int i=0;i<nombreLevrier;i++) 
            { 
            int pos = WaitHandle.WaitAny(arriveesDrapeaux);
                arriveesDrapeaux[pos].Reset();
                resultat[i] = pos;
            }
            //
            AfficherClassement();
            
        }

        private void DeplacerLevrier(int index)
        {
            depart.WaitOne();

            while (distancesParcourues[index] < DistanceCourse)
            {
                mutex.WaitOne();
                try
                {
                    if (distancesParcourues[index] < 2500)
                    {
                        distancesParcourues[index] += random.Next(1, 10);
                    }
                    else
                    {
                        break;
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                if (this.IsHandleCreated)
                {
                    this.Invoke(new Action(() =>
                    {
                        levriers[index].Left = distancesParcourues[index];
                    }));
                }

                Thread.Sleep(50);
            }

            arriveesDrapeaux[index].Set();



           
        }

        private void AfficherClassement()
        {

            string result = "Classement final :\n";

            for (int position = 0;position < resultat.Length; position++)
            {
                result += $"Position {position}: Coureur {resultat[position]}\n";
            }

            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(result, "Classement final", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}

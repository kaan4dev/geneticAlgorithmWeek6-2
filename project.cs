using System;
using System.Collections.Generic;
using System.Linq;

namespace LojistikOptimizasyon
{
    class Program
    {
        // Mesafe matrisi (örnek veri)
        static int[,] mesafeMatrisi = {
            { 0, 29, 20, 21 }, 
            { 29, 0, 15, 17 },
            { 20, 15, 0, 28 },
            { 21, 17, 28, 0 }
        };

        static void Main(string[] args)
        {
            int sehirSayisi = mesafeMatrisi.GetLength(0);
            int popülasyonBoyutu = 10;
            int nesilSayisi = 100;

            List<int[]> popülasyon = PopülasyonBaslat(popülasyonBoyutu, sehirSayisi);
            
            for (int nesil = 0; nesil < nesilSayisi; nesil++)
            {
                // Uygunluk değerlerini hesapla
                var uygunlukDegerleri = popülasyon.Select(rota => RotaMesafesiHesapla(rota)).ToList();
                
                // Seçilim
                var secilenler = EbeveynSec(popülasyon, uygunlukDegerleri);
                
                // Çaprazlama ve mutasyon
                var yeniNesil = new List<int[]>();
                while (yeniNesil.Count < popülasyonBoyutu)
                {
                    int[] ebeveyn1 = secilenler[new Random().Next(secilenler.Count)];
                    int[] ebeveyn2 = secilenler[new Random().Next(secilenler.Count)];

                    int[] cocuk = CaprazlamaYap(ebeveyn1, ebeveyn2);
                    MutasyonYap(cocuk);
                    yeniNesil.Add(cocuk);
                }

                popülasyon = yeniNesil;
            }

            // En iyi rotayı bul ve yazdır
            int[] enIyiRota = popülasyon.OrderBy(rota => RotaMesafesiHesapla(rota)).First();
            Console.WriteLine("En İyi Rota: " + string.Join(" -> ", enIyiRota));
            Console.WriteLine("Toplam Mesafe: " + RotaMesafesiHesapla(enIyiRota));
        }

        static List<int[]> PopülasyonBaslat(int boyut, int sehirSayisi)
        {
            var popülasyon = new List<int[]>();
            for (int i = 0; i < boyut; i++)
            {
                var rota = Enumerable.Range(0, sehirSayisi).OrderBy(x => Guid.NewGuid()).ToArray();
                popülasyon.Add(rota);
            }
            return popülasyon;
        }

        static int RotaMesafesiHesapla(int[] rota)
        {
            int mesafe = 0;
            for (int i = 0; i < rota.Length - 1; i++)
                mesafe += mesafeMatrisi[rota[i], rota[i + 1]];

            mesafe += mesafeMatrisi[rota[^1], rota[0]]; // Dönüş mesafesi
            return mesafe;
        }

        static List<int[]> EbeveynSec(List<int[]> popülasyon, List<int> uygunlukDegerleri)
        {
            return popülasyon.OrderBy(x => uygunlukDegerleri[popülasyon.IndexOf(x)]).Take(popülasyon.Count / 2).ToList();
        }

        static int[] CaprazlamaYap(int[] ebeveyn1, int[] ebeveyn2)
        {
            int boyut = ebeveyn1.Length;
            int[] cocuk = new int[boyut];

            int baslangic = new Random().Next(boyut);
            int bitis = baslangic + new Random().Next(boyut - baslangic);

            Array.Copy(ebeveyn1, baslangic, cocuk, baslangic, bitis - baslangic);

            foreach (var gen in ebeveyn2)
            {
                if (!cocuk.Contains(gen))
                {
                    int indeks = Array.IndexOf(cocuk, 0);
                    cocuk[indeks] = gen;
                }
            }
            return cocuk;
        }

        static void MutasyonYap(int[] rota)
        {
            int indeks1 = new Random().Next(rota.Length);
            int indeks2 = new Random().Next(rota.Length);

            // Swap
            int gecici = rota[indeks1];
            rota[indeks1] = rota[indeks2];
            rota[indeks2] = gecici;
        }
    }
}

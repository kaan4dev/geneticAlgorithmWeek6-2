using System;
using System.Collections.Generic;
using System.Linq;

namespace LojistikOptimizasyon
{
    class Program
    {
        // Sabitler
        const int DepoSayisi = 2;
        const int SehirSayisi = 50;
        const int AracSayisi = 5; // Her depo için araç sayısı (ayarlanabilir)
        const int PopülasyonBoyutu = 50;
        const int NesilSayisi = 200;
        const double MutasyonOrani = 0.05;

        // Mesafe matrisi (örnek veri - gerçek verilerle değiştirin)
        static int[,] mesafeMatrisi = new int[SehirSayisi + DepoSayisi, SehirSayisi + DepoSayisi];

        static void Main(string[] args)
        {
            // Mesafe matrisini doldur (örnek veri)
            MesafeMatrisiOlustur();

            // Genetik algoritma
            List<List<int[]>> popülasyon = PopülasyonBaslat(PopülasyonBoyutu);

            for (int nesil = 0; nesil < NesilSayisi; nesil++)
            {
                // Uygunluk değerlerini hesapla
                var uygunlukDegerleri = popülasyon.Select(birey => ToplamMesafeHesapla(birey)).ToList();

                // Seçilim
                var secilenler = EbeveynSec(popülasyon, uygunlukDegerleri);

                // Çaprazlama ve mutasyon
                var yeniNesil = new List<List<int[]>>();
                while (yeniNesil.Count < PopülasyonBoyutu)
                {
                    List<int[]> ebeveyn1 = secilenler[new Random().Next(secilenler.Count)];
                    List<int[]> ebeveyn2 = secilenler[new Random().Next(secilenler.Count)];

                    List<int[]> cocuk = CaprazlamaYap(ebeveyn1, ebeveyn2);
                    MutasyonYap(cocuk);
                    yeniNesil.Add(cocuk);
                }

                popülasyon = yeniNesil;
            }

            // En iyi çözümü bul ve yazdır
            List<int[]> enIyiCozum = popülasyon.OrderBy(birey => ToplamMesafeHesapla(birey)).First();
            Console.WriteLine("En İyi Çözüm:");
            for (int i = 0; i < DepoSayisi; i++)
            {
                Console.WriteLine($"Depo {i + 1}:");
                for (int j = 0; j < AracSayisi; j++)
                {
                    Console.WriteLine($"  Araç {j + 1}: " + string.Join(" -> ", enIyiCozum[i * AracSayisi + j]));
                }
            }
            Console.WriteLine("Toplam Mesafe: " + ToplamMesafeHesapla(enIyiCozum));
        }

        // Popülasyonu başlat
        static List<List<int[]>> PopülasyonBaslat(int boyut)
        {
            var popülasyon = new List<List<int[]>>();
            for (int i = 0; i < boyut; i++)
            {
                popülasyon.Add(RastgeleBireyOlustur());
            }
            return popülasyon;
        }

        // Rastgele bir birey oluştur (depo ve araç rotaları)
        static List<int[]> RastgeleBireyOlustur()
        {
            var birey = new List<int[]>();
            for (int i = 0; i < DepoSayisi * AracSayisi; i++)
            {
                birey.Add(new int[0]); // Başlangıçta boş rotalar
            }

            // Şehirleri rastgele araçlara ata
            List<int> atanmamisSehirler = Enumerable.Range(DepoSayisi, SehirSayisi).ToList(); // Depoları atla
            Random rnd = new Random();

            while (atanmamisSehirler.Count > 0)
            {
                int sehirIndex = rnd.Next(atanmamisSehirler.Count);
                int sehir = atanmamisSehirler[sehirIndex];
                atanmamisSehirler.RemoveAt(sehirIndex);

                // Rastgele bir depo ve araç seç
                int depo = rnd.Next(DepoSayisi);
                int arac = rnd.Next(AracSayisi);
                int rotaIndex = depo * AracSayisi + arac;

                // Şehri rotaya ekle
                List<int> rota = birey[rotaIndex].ToList();
                rota.Add(sehir);
                birey[rotaIndex] = rota.ToArray();
            }

            return birey;
        }

        // Toplam mesafeyi hesapla (tüm depolar ve araçlar için)
        static int ToplamMesafeHesapla(List<int[]> birey)
        {
            int toplamMesafe = 0;
            for (int i = 0; i < DepoSayisi * AracSayisi; i++)
            {
                int[] rota = birey[i];
                if (rota.Length > 0)
                {
                    int depoIndex = i / AracSayisi; // Hangi depoya ait olduğunu bul
                    toplamMesafe += RotaMesafesiHesapla(depoIndex, rota);
                }
            }
            return toplamMesafe;
        }

        // Rota mesafesini hesapla (depodan başlayıp depoya dön)
        static int RotaMesafesiHesapla(int depoIndex, int[] rota)
        {
            int mesafe = 0;
            if (rota.Length > 0)
            {
                // Depodan ilk şehre
                mesafe += mesafeMatrisi[depoIndex, rota[0]];

                // Şehirler arası
                for (int i = 0; i < rota.Length - 1; i++)
                {
                    mesafe += mesafeMatrisi[rota[i], rota[i + 1]];
                }

                // Son şehirden depoya
                mesafe += mesafeMatrisi[rota[^1], depoIndex];
            }
            return mesafe;
        }

        // Ebeveyn seçimi (rulet tekerleği veya turnuva seçimi kullanılabilir)
        static List<List<int[]>> EbeveynSec(List<List<int[]>> popülasyon, List<int> uygunlukDegerleri)
        {
            // Basit örnek: En iyi %50'yi seç
            return popülasyon.OrderBy(x => uygunlukDegerleri[popülasyon.IndexOf(x)]).Take(PopülasyonBoyutu / 2).ToList();
        }

        // Çaprazlama (iki nokta veya sıralı çaprazlama kullanılabilir)
        static List<int[]> CaprazlamaYap(List<int[]> ebeveyn1, List<int[]> ebeveyn2)
        {
            List<int[]> cocuk = new List<int[]>();
            Random rnd = new Random();

            for (int i = 0; i < DepoSayisi * AracSayisi; i++)
            {
                // Rastgele bir ebeveynden rotayı al
                if (rnd.NextDouble() < 0.5)
                {
                    cocuk.Add((int[])ebeveyn1[i].Clone()); // Deep copy
                }
                else
                {
                    cocuk.Add((int[])ebeveyn2[i].Clone()); // Deep copy
                }
            }

            return cocuk;
        }

        // Mutasyon (rota içinde şehirleri değiştir veya rotaları farklı araçlara taşı)
        static void MutasyonYap(List<int[]> birey)
        {
            Random rnd = new Random();

            if (rnd.NextDouble() < MutasyonOrani)
            {
                // Rastgele bir rota seç
                int rotaIndex = rnd.Next(DepoSayisi * AracSayisi);
                int[] rota = birey[rotaIndex];

                if (rota.Length > 1)
                {
                    // Rastgele iki şehri değiştir
                    int index1 = rnd.Next(rota.Length);
                    int index2 = rnd.Next(rota.Length);

                    int temp = rota[index1];
                    rota[index1] = rota[index2];
                    rota[index2] = temp;
                }
            }
        }

        // Mesafe matrisini oluştur (örnek veri)
        static void MesafeMatrisiOlustur()
        {
            Random rnd = new Random();
            for (int i = 0; i < SehirSayisi + DepoSayisi; i++)
            {
                for (int j = 0; j < SehirSayisi + DepoSayisi; j++)
                {
                    if (i == j)
                    {
                        mesafeMatrisi[i, j] = 0;
                    }
                    else
                    {
                        mesafeMatrisi[i, j] = rnd.Next(10, 100); // Rastgele mesafe
                    }
                }
            }
        }
    }
}
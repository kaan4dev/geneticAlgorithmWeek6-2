using System;
using System.Collections.Generic; 
using System.Linq;

namespace LojistikOptimizasyon
{
    class Program
    {
        const int DepoSayisi = 2; // depo sayısını belirtir
        const int SehirSayisi = 50; // şehir sayısını belirtir
        const int AracSayisi = 5; // her depo için araç sayısı
        const int PopülasyonBoyutu = 50; // popülasyon büyüklüğü
        const int NesilSayisi = 200; // nesil sayısı
        const double MutasyonOrani = 0.05; // mutasyon oranı

        static int[,] mesafeMatrisi = new int[SehirSayisi + DepoSayisi, SehirSayisi + DepoSayisi]; // mesafe matrisi

        static void Main(string[] args)
        {
            MesafeMatrisiOlustur(); // mesafe matrisini oluştur

            // Genetik algoritma
            List<List<List<int[]>>> popülasyon = PopülasyonBaslat(PopülasyonBoyutu); // popülasyonu başlat

            for (int nesil = 0; nesil < NesilSayisi; nesil++) // nesiller boyunca döngü
            {
                // Uygunluk değerlerini hesapla
                var uygunlukDegerleri = popülasyon.Select(birey => ToplamMesafeHesapla(birey)).ToList(); // uygunluk değerlerini hesapla

                // Seçilim
                var secilenler = EbeveynSec(popülasyon, uygunlukDegerleri); // ebeveynleri seç

                // Çaprazlama ve mutasyon
                var yeniNesil = new List<List<List<int[]>>>(); // yeni nesil oluştur
                while (yeniNesil.Count < PopülasyonBoyutu) // popülasyon büyüklüğüne ulaşana kadar döngü
                {
                    List<List<int[]>> ebeveyn1 = secilenler[new Random().Next(secilenler.Count)]; // rastgele ebeveyn seç
                    List<List<int[]>> ebeveyn2 = secilenler[new Random().Next(secilenler.Count)]; // rastgele ebeveyn seç

                    List<List<int[]>> cocuk = CaprazlamaYap(ebeveyn1, ebeveyn2); // çaprazlama yap
                    MutasyonYap(cocuk); // mutasyon yap
                    yeniNesil.Add(cocuk); // yeni nesile ekle
                }

                popülasyon = yeniNesil; // popülasyonu güncelle
            }

            // En iyi çözümü bul ve yazdır
            List<List<int[]>> enIyiCozum = popülasyon.OrderBy(birey => ToplamMesafeHesapla(birey)).First(); // en iyi çözümü bul
            Console.WriteLine("En İyi Çözüm:"); // en iyi çözüm yazdır
            for (int i = 0; i < DepoSayisi; i++)
            {
                Console.WriteLine($"Depo {i + 1}:"); // depo numarasını yazdır
                for (int j = 0; j < AracSayisi; j++) // araçlar boyunca döngü
                {
                    Console.WriteLine($"  Araç {j + 1}: " + string.Join(" -> ", enIyiCozum[i][j])); // araç rotasını yazdır
                }
            }
            Console.WriteLine("Toplam Mesafe: " + ToplamMesafeHesapla(enIyiCozum)); // toplam mesafeyi yazdır
        }

        // Popülasyonu başlat
        static List<List<List<int[]>>> PopülasyonBaslat(int boyut) // popülasyonu başlatma fonksiyonu
        {
            var popülasyon = new List<List<List<int[]>>>(); // popülasyon oluştur
            for (int i = 0; i < boyut; i++) // popülasyon büyüklüğü kadar döngü
            {
                popülasyon.Add(RastgeleBireyOlustur()); // rastgele birey oluştur ve ekle
            }
            return popülasyon; // popülasyonu döndür
        }

        static List<List<int[]>> RastgeleBireyOlustur() // rastgele birey oluşturma fonksiyonu
        {
            var birey = new List<List<int[]>>(); // birey oluştur
            for (int i = 0; i < DepoSayisi; i++) // depolar boyunca döngü
            {
                var depoRotalari = new List<int[]>(); // depo rotaları oluştur
                for (int j = 0; j < AracSayisi; j++) // araçlar boyunca döngü
                {
                    depoRotalari.Add(new int[0]); // başlangıçta boş rotalar
                }
                birey.Add(depoRotalari); // depo rotalarını bireye ekle
            }

            List<int> atanmamisSehirler = Enumerable.Range(DepoSayisi, SehirSayisi).ToList(); // atanmamış şehirler listesi
            Random rnd = new Random(); // rastgele sayı üreteci

            while (atanmamisSehirler.Count > 0) // atanmamış şehirler varken döngü
            {
                int sehirIndex = rnd.Next(atanmamisSehirler.Count); // rastgele şehir indeksi seç
                int sehir = atanmamisSehirler[sehirIndex]; // şehri al
                atanmamisSehirler.RemoveAt(sehirIndex); // şehri listeden çıkar

                int depo = rnd.Next(DepoSayisi); // rastgele depo seç
                int arac = rnd.Next(AracSayisi); // rastgele araç seç
                
                List<int> rota = birey[depo][arac].ToList(); // rotayı al
                rota.Add(sehir); // şehri rotaya ekle
                birey[depo][arac] = rota.ToArray(); // rotayı güncelle
            }

            SehirlerinZiyaretEdildigindenEminOl(birey); // şehirlerin ziyaret edildiğinden emin ol

            return birey; // bireyi döndür
        }

        static int ToplamMesafeHesapla(List<List<int[]>> birey) // toplam mesafeyi hesaplama fonksiyonu
        {
            int toplamMesafe = 0; // toplam mesafe
            for (int i = 0; i < DepoSayisi; i++) // depolar boyunca döngü
            {
                for (int j = 0; j < AracSayisi; j++) // araçlar boyunca döngü
                {
                    int[] rota = birey[i][j]; // rotayı al
                    if (rota.Length > 0) // rota boş değilse
                    {
                        toplamMesafe += RotaMesafesiHesapla(i, rota); // depoIndex = i rotanın mesafesini hesapla ve ekle
                    }
                }
            }
            return toplamMesafe; // toplam mesafeyi döndür
        }

        static int RotaMesafesiHesapla(int depoIndex, int[] rota) // rota mesafesini hesaplama fonksiyonu
        {
            int mesafe = 0; // mesafe
            if (rota.Length > 0) // rota boş değilse
            {
                // Depodan ilk şehre
                mesafe += mesafeMatrisi[depoIndex, rota[0]]; // depodan ilk şehre mesafeyi ekle

                // Şehirler arası
                for (int i = 0; i < rota.Length - 1; i++) // şehirler arası döngü
                {
                    mesafe += mesafeMatrisi[rota[i], rota[i + 1]]; // şehirler arası mesafeyi ekle
                }

                // Son şehirden depoya
                mesafe += mesafeMatrisi[rota[^1], depoIndex]; // son şehirden depoya mesafeyi ekle
            }
            return mesafe; // mesafeyi döndür
        }

        static List<List<List<int[]>>> EbeveynSec(List<List<List<int[]>>> popülasyon, List<int> uygunlukDegerleri) // ebeveyn seçimi fonksiyonu
        {
            // Basit örnek: En iyi %50'yi seç
            return popülasyon.OrderBy(x => uygunlukDegerleri[popülasyon.IndexOf(x)]).Take(PopülasyonBoyutu / 2).ToList(); // en iyi %50'yi seç
        }

        static List<List<int[]>> CaprazlamaYap(List<List<int[]>> ebeveyn1, List<List<int[]>> ebeveyn2) // çaprazlama fonksiyonu
        {
            List<List<int[]>> cocuk = new List<List<int[]>>(); // çocuk oluştur
            Random rnd = new Random(); // rastgele sayı üreteci

            for (int i = 0; i < DepoSayisi; i++) // depolar boyunca döngü
            {
                var depoRotalari = new List<int[]>(); // depo rotaları oluştur
                for (int j = 0; j < AracSayisi; j++) // araçlar boyunca döngü
                {
                    // Rastgele bir ebeveynden rotayı al
                    if (rnd.NextDouble() < 0.5) // %50 olasılıkla
                    {
                        depoRotalari.Add((int[])ebeveyn1[i][j].Clone()); // deep copy ebeveyn1'den rotayı al
                    }
                    else
                    {
                        depoRotalari.Add((int[])ebeveyn2[i][j].Clone()); // deep copy ebeveyn2'den rotayı al
                    }
                }
                cocuk.Add(depoRotalari); // depo rotalarını çocuğa ekle
            }

            return cocuk; // çocuğu döndür
        }

        static void MutasyonYap(List<List<int[]>> birey) // mutasyon fonksiyonu
        {
            Random rnd = new Random(); // rastgele sayı üreteci

            if (rnd.NextDouble() < MutasyonOrani) // mutasyon oranı ile karşılaştır
            {
                // Rastgele bir depo seç
                int depoIndex = rnd.Next(DepoSayisi); // rastgele depo indeksi seç
                // Rastgele bir rota seç
                int rotaIndex = rnd.Next(AracSayisi); // rastgele rota indeksi seç
                int[] rota = birey[depoIndex][rotaIndex]; // rotayı al

                if (rota.Length > 1) // rota uzunluğu 1'den büyükse
                {
                    // Rastgele iki şehri değiştir
                    int index1 = rnd.Next(rota.Length); // rastgele indeks 1 seç
                    int index2 = rnd.Next(rota.Length); // rastgele indeks 2 seç

                    int temp = rota[index1]; // geçici değişkene al
                    rota[index1] = rota[index2]; // değiştir
                    rota[index2] = temp; // değiştir
                }
            }
        }

        static void MesafeMatrisiOlustur() // mesafe matrisini oluşturma fonksiyonu
        {
            Random rnd = new Random(); // rastgele sayı üreteci
            for (int i = 0; i < SehirSayisi + DepoSayisi; i++) // şehirler ve depolar boyunca döngü
            {
                for (int j = 0; j < SehirSayisi + DepoSayisi; j++) // şehirler ve depolar boyunca döngü
                {
                    if (i == j) // aynı şehir ise
                    {
                        mesafeMatrisi[i, j] = 0; // mesafe 0
                    }
                    else
                    {
                        mesafeMatrisi[i, j] = rnd.Next(10, 100); // rastgele mesafe
                    }
                }
            }
        }

        static void SehirlerinZiyaretEdildigindenEminOl(List<List<int[]>> birey) // şehirlerin ziyaret edildiğinden emin olma fonksiyonu
        {
            List<int> ziyaretEdilmeyenSehirler = Enumerable.Range(DepoSayisi, SehirSayisi).ToList(); // ziyaret edilmeyen şehirler listesi (depoları atla)

            // Ziyaret edilen şehirleri bul
            foreach (var depoRotalari in birey) // depolar boyunca döngü
            {
                foreach (var rota in depoRotalari) // rotalar boyunca döngü
                {
                    foreach (int sehir in rota) // şehirler boyunca döngü
                    {
                        ziyaretEdilmeyenSehirler.Remove(sehir); // ziyaret edilmeyen şehirlerden çıkar
                    }
                }
            }

            Random rnd = new Random(); // rastgele sayı üreteci
            while (ziyaretEdilmeyenSehirler.Count > 0) // ziyaret edilmeyen şehirler varken döngü
            {
                int sehirIndex = rnd.Next(ziyaretEdilmeyenSehirler.Count); // rastgele şehir indeksi seç
                int sehir = ziyaretEdilmeyenSehirler[sehirIndex]; // şehri al
                ziyaretEdilmeyenSehirler.RemoveAt(sehirIndex); // listeden çıkar

                // Rastgele bir depo ve araç seç
                int depo = rnd.Next(DepoSayisi); // rastgele depo seç
                int arac = rnd.Next(AracSayisi); // rastgele araç seç

                // Şehri rotaya ekle
                List<int> rota = birey[depo][arac].ToList(); // rotayı al
                rota.Add(sehir); // şehri ekle
                birey[depo][arac] = rota.ToArray(); // rotayı güncelle
            }
        }
    }
}
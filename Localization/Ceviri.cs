namespace InternshipMpbile.Localization
{
    public enum Dil
    {
        Turkce,
        Ingilizce
    }

    /// <summary>
    /// Uygulamanın arayüz metinlerini seçili dile çevirir.
    ///
    /// Anahtar olarak metnin Türkçesi kullanılır: XAML ve kod Türkçe okunur kalır,
    /// Türkçe modda hiç sözlüğe bakılmaz ve karşılığı yazılmamış bir metin boş
    /// görünmek yerine Türkçe haliyle görünür.
    ///
    /// Dil, uygulama açıkken bellekte tutulur; her açılışta Türkçe başlar.
    /// Değiştirildiğinde arayüzün yeniden kurulması gerekir (bkz. App.DiliDegistir).
    /// </summary>
    public static class Ceviri
    {
        public static Dil AktifDil { get; set; } = Dil.Turkce;

        /// <summary>
        /// Arayüz metinleri ile açılır listelerdeki sabit seçeneklerin karşılıkları.
        ///
        /// Seçenekler yalnızca ekranda çevrilir: veritabanına her zaman Türkçe değer
        /// yazılır ve oradan Türkçe okunur. Kullanıcının kendi yazdığı değerler
        /// (referans alt tipleri, proje adları) burada bulunmadığı için olduğu gibi
        /// görünür.
        /// </summary>
        private static readonly Dictionary<string, string> IngilizceKarsiliklar = new()
        {
            // ---------- Açılır liste seçenekleri ----------
            // Başvuran birim
            ["Bilgi İşlem"] = "IT",
            ["İnsan Kaynakları"] = "Human Resources",
            ["Yatırım İşleri"] = "Investment Affairs",

            // Başvuru yapılan proje / tür
            ["Merkezi"] = "Central",
            ["Avrupa"] = "Europe",
            ["Diğer"] = "Other",
            ["Gençlik"] = "Youth",
            ["Yetişkin"] = "Adult",
            ["Spor"] = "Sports",
            ["Mesleki"] = "Vocational",
            ["Dijital"] = "Digital",

            // Katılımcı türü
            ["Koordinatör"] = "Coordinator",
            ["Ortak"] = "Partner",

            // Başvuru durumu
            ["Kabul"] = "Accepted",
            ["Red"] = "Rejected",

            // Roller
            ["Kullanıcı"] = "User",
            ["Personel"] = "Staff",

            // Referans tipleri (Referans tablosundaki Type sütununun değerleri)
            ["Başvuran Birim"] = "Applying Unit",
            ["Başvuru Yapılan Proje"] = "Applied Project",
            ["Başvuru Yapılan Tür"] = "Applied Type",
            ["Katılımcı Türü"] = "Participant Type",
            ["Başvuru Dönemi"] = "Application Period",

            // ---------- Menü ----------
            ["Başvuru Formu"] = "Application Form",
            ["Başvuru Listesi"] = "Application List",
            ["Referans Ekleme"] = "Add Reference",
            ["Referans Listesi"] = "Reference List",
            ["Kullanıcılar"] = "Users",
            ["GİRİŞ YAPAN KULLANICI"] = "SIGNED IN USER",
            ["Çıkış Yap"] = "Log Out",

            // ---------- Dil seçimi ----------
            ["Türkçe"] = "Turkish",
            ["İngilizce"] = "English",
            ["Karanlık Mod"] = "Dark Mode",

            // ---------- Ortak ----------
            ["Kaydet"] = "Save",
            ["Gönder"] = "Send",
            ["Vazgeç"] = "Cancel",
            ["Evet"] = "Yes",
            ["Hayır"] = "No",
            ["Tamam"] = "OK",
            ["Hata"] = "Error",
            ["Seçiniz"] = "Select",
            ["Tümü"] = "All",
            ["gg.aa.yyyy"] = "dd.mm.yyyy",
            ["E-POSTA"] = "E-MAIL",
            ["PAROLA"] = "PASSWORD",

            // ---------- Giriş ekranı ----------
            ["Giriş Yap"] = "Log In",
            ["Şifremi Unuttum / Şifre almak istiyorum"] = "Forgot My Password / I want a password",
            ["Şifre Alma"] = "Get Password",
            ["Geçici parolanızın gönderileceği e-posta adresini yazınız."] =
                "Enter the e-mail address your temporary password will be sent to.",
            ["Lütfen e-posta ve parolanızı giriniz."] = "Please enter your e-mail and password.",
            ["E-posta veya parola hatalı."] = "E-mail or password is incorrect.",
            ["Giriş sırasında bir hata oluştu:"] = "An error occurred while signing in:",
            ["Lütfen geçerli bir e-posta adresi giriniz."] = "Please enter a valid e-mail address.",
            ["Bu e-posta ile kayıtlı bir kullanıcı bulunamadı."] = "No user is registered with this e-mail.",
            ["Geçici parolanız e-posta adresinize gönderildi. Giriş yaptıktan sonra yeni parolanızı belirleyebilirsiniz."] =
                "Your temporary password has been sent to your e-mail address. You can set a new password after signing in.",
            ["Geçici parola gönderilemedi:"] = "Temporary password could not be sent:",

            // ---------- Başvuru formu ----------
            ["PROJE ADI"] = "PROJECT NAME",
            ["BAŞVURAN BİRİM"] = "APPLYING UNIT",
            ["BAŞVURU YAPILAN PROJE"] = "APPLIED PROJECT",
            ["BAŞVURU YAPILAN TÜR"] = "APPLIED TYPE",
            ["KATILIMCI TÜRÜ"] = "PARTICIPANT TYPE",
            ["BAŞVURU DÖNEMİ"] = "APPLICATION PERIOD",
            ["BAŞVURU TARİHİ"] = "APPLICATION DATE",
            ["BAŞVURU DURUMU"] = "APPLICATION STATUS",
            ["DURUM TARİHİ"] = "STATUS DATE",
            ["HİBE TUTARI"] = "GRANT AMOUNT",
            ["Lütfen tüm zorunlu alanları doldurun."] = "Please fill in all required fields.",
            ["Hibe tutarı 0'dan büyük bir sayı olmalıdır."] = "Grant amount must be a number greater than 0.",
            ["Durum tarihi, başvuru tarihinden ileri bir tarih olamaz."] =
                "Status date cannot be later than the application date.",
            ["Bu proje adı daha önce kullanılmış. Lütfen farklı bir proje adı giriniz."] =
                "This project name has already been used. Please enter a different project name.",
            ["Başvuru başarıyla kaydedildi."] = "The application was saved successfully.",
            ["Kayıt sırasında bir hata oluştu:"] = "An error occurred while saving:",
            ["Referanslar yüklenirken bir hata oluştu:"] = "An error occurred while loading references:",

            // ---------- Parola değiştirme ----------
            ["Parolayı Değiştir"] = "Change Password",
            ["Geçici parolanızla giriş yaptınız. Devam etmek için yeni parolanızı belirleyiniz."] =
                "You signed in with your temporary password. Please set a new password to continue.",
            ["ESKİ PAROLA"] = "OLD PASSWORD",
            ["YENİ PAROLA"] = "NEW PASSWORD",
            ["YENİ PAROLA (TEKRAR)"] = "NEW PASSWORD (REPEAT)",
            ["Lütfen tüm alanları doldurun."] = "Please fill in all fields.",
            ["Yeni parolalar birbiriyle aynı değil."] = "The new passwords do not match.",
            ["Yeni parola en az {0} karakter olmalıdır."] = "The new password must be at least {0} characters.",
            ["Yeni parola geçici parolanızdan farklı olmalıdır."] =
                "The new password must be different from your temporary password.",
            ["Eski parolanız hatalı."] = "Your old password is incorrect.",
            ["Parolanız başarıyla değiştirildi."] = "Your password was changed successfully.",
            ["Parola değiştirilirken bir hata oluştu:"] = "An error occurred while changing the password:",

            // ---------- Başvuru listesi ----------
            ["FİLTRELE"] = "FILTER",
            ["Filtrele"] = "Filter",
            ["Temizle"] = "Clear",
            ["YAPILAN PROJE"] = "APPLIED PROJECT",
            ["YAPILAN TÜR"] = "APPLIED TYPE",
            ["Henüz başvuru yok"] = "No applications yet",
            ["Başvuru Formu ekranından yeni bir kayıt oluşturabilirsiniz."] =
                "You can create a new record from the Application Form screen.",
            ["Filtreye uyan başvuru yok"] = "No applications match the filter",
            ["Filtreyi değiştirebilir ya da Temizle ile tüm başvuruları görebilirsiniz."] =
                "You can change the filter or use Clear to see all applications.",
            ["Hibe tutarı için geçerli bir sayı giriniz."] = "Please enter a valid number for the grant amount.",
            ["Başvurular yüklenirken bir hata oluştu:"] = "An error occurred while loading applications:",

            // ---------- Referans ekleme / listesi ----------
            ["REFERANS TİPİ"] = "REFERENCE TYPE",
            ["REFERANS ALT TİPİ"] = "REFERENCE SUBTYPE",
            ["Lütfen referans tipini seçip alt tipini yazınız."] = "Please select a reference type and enter a subtype.",
            ["Bu referans zaten kayıtlı."] = "This reference is already registered.",
            ["Referans başarıyla kaydedildi."] = "The reference was saved successfully.",
            ["Referansı Silmek İstediğinizden Emin Misiniz?"] = "Are You Sure You Want To Delete This Reference?",
            ["Henüz referans yok"] = "No references yet",
            ["Referans Ekleme ekranından yeni bir kayıt oluşturabilirsiniz."] =
                "You can create a new record from the Add Reference screen.",
            ["Filtreye uyan referans yok"] = "No references match the filter",
            ["Filtreyi değiştirebilir ya da Temizle ile tüm referansları görebilirsiniz."] =
                "You can change the filter or use Clear to see all references.",
            ["Silme sırasında bir hata oluştu:"] = "An error occurred while deleting:",

            // ---------- Kullanıcılar ----------
            ["Kullanıcı Ekleme"] = "Add User",
            ["Kullanıcı Listesi"] = "User List",
            ["İSİM"] = "FIRST NAME",
            ["SOYİSİM"] = "LAST NAME",
            ["ŞİFRE"] = "PASSWORD",
            ["ROL"] = "ROLE",
            ["Henüz kullanıcı yok"] = "No users yet",
            ["Yukarıdaki formdan yeni bir kullanıcı ekleyebilirsiniz."] =
                "You can add a new user from the form above.",
            ["Bu Kullanıcıyı Kaydetmek İstediğinize Emin Misiniz?"] = "Are You Sure You Want To Save This User?",
            ["Kullanıcıyı Silmek İstediğinizden Emin Misiniz?"] = "Are You Sure You Want To Delete This User?",
            ["Bu e-posta ile kayıtlı bir kullanıcı zaten var."] = "A user with this e-mail already exists.",
            ["Kullanıcı başarıyla eklendi."] = "The user was added successfully.",
            ["Kullanıcılar yüklenirken bir hata oluştu:"] = "An error occurred while loading users:"
        };

        /// <summary>
        /// Verilen Türkçe metnin seçili dildeki karşılığını döndürür.
        /// Karşılığı yoksa metin olduğu gibi geri verilir.
        /// </summary>
        public static string Al(string turkce)
        {
            if (AktifDil == Dil.Turkce)
                return turkce;

            return IngilizceKarsiliklar.TryGetValue(turkce, out var karsilik) ? karsilik : turkce;
        }

        /// <summary>Yer tutucu ({0}) içeren metinler için.</summary>
        public static string Al(string turkce, params object[] degerler) =>
            string.Format(Al(turkce), degerler);
    }
}

using System;
using System.IO;
using System.Xml.Serialization;

namespace TabelloneWPF
{
    public sealed class ConfigurazioneService
    {
        public string CartellaConfigurazione { get; } = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "lastconfig");

        public string CartellaDati { get; } = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data");

        public string PercorsoConfigurazioneSquadre => Path.Combine(
            CartellaConfigurazione,
            "config_squadre.xml");

        public string PercorsoConfigurazioneColori => Path.Combine(
            CartellaDati,
            "ConfColor.xml");

        public string PercorsoInformazioniPartita => Path.Combine(
            CartellaConfigurazione,
            "info_partita.xml");

        public T? Carica<T>(string percorso)
        {
            if (!File.Exists(percorso))
                return default;

            using var reader = new StreamReader(percorso);
            var serializer = new XmlSerializer(typeof(T));
            return (T?)serializer.Deserialize(reader);
        }

        public void Salva<T>(string percorso, T modello)
        {
            string? cartella = Path.GetDirectoryName(percorso);
            if (!string.IsNullOrEmpty(cartella))
                Directory.CreateDirectory(cartella);

            using var writer = new StreamWriter(percorso);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, modello);
        }

        public string RisolviPercorsoFileConfigurazione(string? percorsoSalvato)
        {
            if (string.IsNullOrWhiteSpace(percorsoSalvato))
                return "";

            string percorsoInConfigurazione = Path.Combine(
                CartellaConfigurazione,
                percorsoSalvato);

            if (File.Exists(percorsoInConfigurazione))
                return percorsoInConfigurazione;

            if (File.Exists(percorsoSalvato))
                return percorsoSalvato;

            string percorsoRelativo = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                percorsoSalvato);

            return File.Exists(percorsoRelativo)
                ? percorsoRelativo
                : percorsoSalvato;
        }
    }
}

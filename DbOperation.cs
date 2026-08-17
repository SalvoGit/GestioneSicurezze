using Dapper;
using GestioneSicurezze.Models;
using Npgsql;
using System.Configuration;
using System.Data;

namespace GestioneSicurezze
{
    public class DbOperation
    {
        /************* CONNESSIONE AL DATABASE SVILUPPO*************/
        //private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["GestioneSicurezzeDbSviluppo"].ConnectionString;
        //private static readonly bool isProd = false;

        /************* CONNESSIONE AL DATABASE PRODUZIONE*************/
        private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["GestioneSicurezzeDbProd"].ConnectionString;
        private static readonly bool isProd = true;

        public static IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
        public static string messageError = string.Empty;
        public static string GetErrorMessage => messageError;
        static IDbConnection connection;

        #region "Metodo per testare la connessione al database"
        public static bool TestConnection()
        {
            using var connection = CreateConnection();
            try
            {
                connection.Open();
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception("Database connection failed: " + ex.Message);
                return false;
            }
        }
        #endregion
        #region "OpenConnection e CloseConnection"
        public static bool OpenConnection()
        {
            connection = CreateConnection();
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                messageError = $"Impossibile connettersi al database. Controllare la connessione di rete o contattare l'amministratore di sistema.\nErrore : {ex.Message}";
                return false;
            }
        }
        public static bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                messageError = $"Impossibile chiudere la connessione al Database.\nErrore : {ex.Message}";
                return false;
            }
        }
        #endregion

        #region "Metodo per recuperare il prossimo progressivo"
        public static int GetNextProgressivo()
        {
            //using var connection = CreateConnection();
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;

                    if (isProd)
                    {
                        sqlCommand = "SELECT COALESCE(MAX(\"PROGRESSIVO\"), 0) + 1 FROM sicurezze.\"SicurRegistroSicurezze\"";
                    }
                    else
                    {
                        sqlCommand = "SELECT COALESCE(MAX(\"PROGRESSIVO\"), 0) + 1 FROM mezzapesa.\"SicurRegistroSicurezze\"";
                    }

                    int nextProgressivo = conn.ExecuteScalar<int>(sqlCommand);
                    return nextProgressivo;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero del progressivo.\nErrore : {ex.Message}";
                    return -1;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        #endregion

        #region "Metodo per verificare se esiste un record con lo stesso AWB negli ultimi 30 minuti"
        public static bool ThirtyMinutesCheck(string awb)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = "SELECT COUNT(*) FROM sicurezze.\"SicurRegistroSicurezze\" WHERE \"AWB\" = @AWB AND \"DATAESECUZIONE\" >= NOW() - INTERVAL '30 minutes' AND \"DATAESECUZIONE\" < NOW()";
                    }
                    else
                    {
                        sqlCommand = "SELECT COUNT(*) FROM mezzapesa.\"SicurRegistroSicurezze\" WHERE \"AWB\" = @AWB AND \"DATAESECUZIONE\" >= NOW() - INTERVAL '30 minutes' AND \"DATAESECUZIONE\" < NOW()";
                    }
                    int count = conn.ExecuteScalar<int>(sqlCommand, new { AWB = awb });
                    return count > 0;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante la verifica del controllo 30 minuti.\nErrore : {ex.Message}";
                    return false;
                }
            }
        }
        #endregion

        #region "Metodo per verificare se esiste un record con lo stesso AWB"
        public static bool CheckIfAWBExists(string awb)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = "SELECT COUNT(*) FROM sicurezze.\"SicurRegistroSicurezze\" WHERE \"AWB\" = @AWB";
                    }
                    else
                    {
                        sqlCommand = "SELECT COUNT(*) FROM mezzapesa.\"SicurRegistroSicurezze\" WHERE \"AWB\" = @AWB";
                    }
                    int count = conn.ExecuteScalar<int>(sqlCommand, new { AWB = awb });
                    return count > 0;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante la verifica dell'esistenza del progressivo.\nErrore : {ex.Message}";
                    return false;
                }
            }
        }
        #endregion

        #region "Metodo per inserire un nuovo record nel database"
        public static DBResult InsertSicurezzaDBNoProg(ModelloXray modelloXray)
        {
            using (IDbConnection conn = CreateConnection())
            {
                DBResult result = new();
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                        INSERT INTO sicurezze."SicurRegistroSicurezze" ("NRENTRATA", "CODICEENAC", "AWB", "COLLI", "PESO", "DESTINAZIONE", 
                        "CONTENUTO", "CLIENTE", "OPERATORE", "DATAESECUZIONE", "RIFERIMENTO", "TRASPORTATORE", "TARGA", "SIGILLONUMERO",
                        "AUTISTA", "XRAY", "ETD", "PHS", "VCK", "STATOMERCE", "AEROPORTODEST","QT_XRAY","QT_ETD","QT_PHS","QT_VCK")
                        VALUES (@NrEntrata, @CodiceEnac, @Awb, @Colli, @Peso, @Destinazione, @Contenuto, @Cliente, @Operatore, 
                        @DataEsecuzione, @Riferimento, @Trasportatore, @Targa, @SigilloNumero, @Autista, @XRAY, @ETD, @PHS, @VCK, @STATOMERCE, @AeroportoDest,
                        @QT_XRAY,@QT_ETD,@QT_PHS,@QT_VCK)
                        RETURNING "ID","PROGRESSIVO";
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                        INSERT INTO mezzapesa."SicurRegistroSicurezze" ("NRENTRATA", "CODICEENAC", "AWB", "COLLI", "PESO", "DESTINAZIONE", 
                        "CONTENUTO", "CLIENTE", "OPERATORE", "DATAESECUZIONE", "RIFERIMENTO", "TRASPORTATORE", "TARGA", "SIGILLONUMERO",
                        "AUTISTA", "XRAY", "ETD", "PHS", "VCK", "STATOMERCE", "AEROPORTODEST","QT_XRAY","QT_ETD","QT_PHS","QT_VCK")
                        VALUES (@NrEntrata, @CodiceEnac, @Awb, @Colli, @Peso, @Destinazione, @Contenuto, @Cliente, @Operatore, 
                        @DataEsecuzione, @Riferimento, @Trasportatore, @Targa, @SigilloNumero, @Autista, @XRAY, @ETD, @PHS, @VCK, @STATOMERCE, @AeroportoDest,
                        @QT_XRAY,@QT_ETD,@QT_PHS,@QT_VCK)
                        RETURNING "ID","PROGRESSIVO";
                    
                    """;
                    }

                    return result = conn.QuerySingle<DBResult>(sqlCommand, modelloXray);
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento nel database.\nErrore : {ex.Message}";
                    result.ID = -1;
                    return result;
                }
            }

        }
        #endregion

        #region "Metodo per inserire un nuovo record nel database"
        public static int InsertSicurezzaDB(ModelloXray modelloXray)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                        INSERT INTO sicurezze."SicurRegistroSicurezze" ("PROGRESSIVO", "NRENTRATA", "CODICEENAC", "AWB", "COLLI", "PESO", "DESTINAZIONE", 
                        "CONTENUTO", "CLIENTE", "OPERATORE", "DATAESECUZIONE", "RIFERIMENTO", "TRASPORTATORE", "TARGA", "SIGILLONUMERO",
                        "AUTISTA", "XRAY", "ETD", "PHS", "VCK", "STATOMERCE", "AEROPORTODEST","QT_XRAY","QT_ETD","QT_PHS","QT_VCK")
                        VALUES (@Progressivo, @NrEntrata, @CodiceEnac, @Awb, @Colli, @Peso, @Destinazione, @Contenuto, @Cliente, @Operatore, 
                        @DataEsecuzione, @Riferimento, @Trasportatore, @Targa, @SigilloNumero, @Autista, @XRAY, @ETD, @PHS, @VCK, @STATOMERCE, @AeroportoDest,
                        @QT_XRAY,@QT_ETD,@QT_PHS,@QT_VCK)
                        RETURNING "ID"
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                        INSERT INTO mezzapesa."SicurRegistroSicurezze" ("PROGRESSIVO", "NRENTRATA", "CODICEENAC", "AWB", "COLLI", "PESO", "DESTINAZIONE", 
                        "CONTENUTO", "CLIENTE", "OPERATORE", "DATAESECUZIONE", "RIFERIMENTO", "TRASPORTATORE", "TARGA", "SIGILLONUMERO",
                        "AUTISTA", "XRAY", "ETD", "PHS", "VCK", "STATOMERCE", "AEROPORTODEST","QT_XRAY","QT_ETD","QT_PHS","QT_VCK")
                        VALUES (@Progressivo, @NrEntrata, @CodiceEnac, @Awb, @Colli, @Peso, @Destinazione, @Contenuto, @Cliente, @Operatore, 
                        @DataEsecuzione, @Riferimento, @Trasportatore, @Targa, @SigilloNumero, @Autista, @XRAY, @ETD, @PHS, @VCK, @STATOMERCE, @AeroportoDest,
                        @QT_XRAY,@QT_ETD,@QT_PHS,@QT_VCK)
                        RETURNING "ID"
                    
                    """;
                    }

                    int idGenerato = conn.ExecuteScalar<int>(sqlCommand, modelloXray);
                    return idGenerato;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento nel database.\nErrore : {ex.Message}";
                    return -1;
                }
            }

        }
        #endregion

        #region "Metodo per inserire un nuovo record nel database"
        public static DBResult InsertSicurezzaWithConflictDB(ModelloXray modelloXray)
        {
            using (IDbConnection conn = CreateConnection())
            {
                DBResult result = new();
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                        INSERT INTO sicurezze."SicurRegistroSicurezze" ("PROGRESSIVO", "NRENTRATA", "CODICEENAC", "AWB", "COLLI", "PESO", "DESTINAZIONE", 
                        "CONTENUTO", "CLIENTE", "OPERATORE", "DATAESECUZIONE", "RIFERIMENTO", "TRASPORTATORE", "TARGA", "SIGILLONUMERO",
                        "AUTISTA", "XRAY", "ETD", "PHS", "VCK", "STATOMERCE", "AEROPORTODEST","QT_XRAY","QT_ETD","QT_PHS","QT_VCK")
                        VALUES (@Progressivo, @NrEntrata, @CodiceEnac, @Awb, @Colli, @Peso, @Destinazione, @Contenuto, @Cliente, @Operatore, 
                        @DataEsecuzione, @Riferimento, @Trasportatore, @Targa, @SigilloNumero, @Autista, @XRAY, @ETD, @PHS, @VCK, @STATOMERCE, @AeroportoDest,
                        @QT_XRAY,@QT_ETD,@QT_PHS,@QT_VCK)
                        ON CONFLICT ("PROGRESSIVO")                                            
                        DO UPDATE SET
                            "NRENTRATA"     = EXCLUDED."NRENTRATA",
                            "CODICEENAC"    = EXCLUDED."CODICEENAC",
                            "AWB"           = EXCLUDED."AWB",
                            "COLLI"         = EXCLUDED."COLLI",
                            "PESO"          = EXCLUDED."PESO",
                            "DESTINAZIONE"  = EXCLUDED."DESTINAZIONE",
                            "CONTENUTO"     = EXCLUDED."CONTENUTO",
                            "CLIENTE"       = EXCLUDED."CLIENTE",
                            "OPERATORE"     = EXCLUDED."OPERATORE",
                            "DATAESECUZIONE"= EXCLUDED."DATAESECUZIONE",
                            "RIFERIMENTO"   = EXCLUDED."RIFERIMENTO",
                            "TRASPORTATORE" = EXCLUDED."TRASPORTATORE",
                            "TARGA"         = EXCLUDED."TARGA",
                            "SIGILLONUMERO" = EXCLUDED."SIGILLONUMERO",
                            "AUTISTA"       = EXCLUDED."AUTISTA",
                            "XRAY"          = EXCLUDED."XRAY",
                            "ETD"           = EXCLUDED."ETD",
                            "PHS"           = EXCLUDED."PHS",
                            "VCK"           = EXCLUDED."VCK",
                            "STATOMERCE"    = EXCLUDED."STATOMERCE",
                            "AEROPORTODEST" = EXCLUDED."AEROPORTODEST",
                            "QT_XRAY"       = EXCLUDED."QT_XRAY",
                            "QT_ETD"        = EXCLUDED."QT_ETD",
                            "QT_PHS"        = EXCLUDED."QT_PHS",
                            "QT_VCK"        = EXCLUDED."QT_VCK"
                        RETURNING "ID","PROGRESSIVO";
                    
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                        INSERT INTO mezzapesa."SicurRegistroSicurezze" ("PROGRESSIVO", "NRENTRATA", "CODICEENAC", "AWB", "COLLI", "PESO", "DESTINAZIONE", 
                        "CONTENUTO", "CLIENTE", "OPERATORE", "DATAESECUZIONE", "RIFERIMENTO", "TRASPORTATORE", "TARGA", "SIGILLONUMERO",
                        "AUTISTA", "XRAY", "ETD", "PHS", "VCK", "STATOMERCE", "AEROPORTODEST","QT_XRAY","QT_ETD","QT_PHS","QT_VCK")
                        VALUES (@Progressivo, @NrEntrata, @CodiceEnac, @Awb, @Colli, @Peso, @Destinazione, @Contenuto, @Cliente, @Operatore, 
                        @DataEsecuzione, @Riferimento, @Trasportatore, @Targa, @SigilloNumero, @Autista, @XRAY, @ETD, @PHS, @VCK, @STATOMERCE, @AeroportoDest,
                        @QT_XRAY,@QT_ETD,@QT_PHS,@QT_VCK)
                        ON CONFLICT ("PROGRESSIVO")                                            
                        DO UPDATE SET
                            "NRENTRATA"     = EXCLUDED."NRENTRATA",
                            "CODICEENAC"    = EXCLUDED."CODICEENAC",
                            "AWB"           = EXCLUDED."AWB",
                            "COLLI"         = EXCLUDED."COLLI",
                            "PESO"          = EXCLUDED."PESO",
                            "DESTINAZIONE"  = EXCLUDED."DESTINAZIONE",
                            "CONTENUTO"     = EXCLUDED."CONTENUTO",
                            "CLIENTE"       = EXCLUDED."CLIENTE",
                            "OPERATORE"     = EXCLUDED."OPERATORE",
                            "DATAESECUZIONE"= EXCLUDED."DATAESECUZIONE",
                            "RIFERIMENTO"   = EXCLUDED."RIFERIMENTO",
                            "TRASPORTATORE" = EXCLUDED."TRASPORTATORE",
                            "TARGA"         = EXCLUDED."TARGA",
                            "SIGILLONUMERO" = EXCLUDED."SIGILLONUMERO",
                            "AUTISTA"       = EXCLUDED."AUTISTA",
                            "XRAY"          = EXCLUDED."XRAY",
                            "ETD"           = EXCLUDED."ETD",
                            "PHS"           = EXCLUDED."PHS",
                            "VCK"           = EXCLUDED."VCK",
                            "STATOMERCE"    = EXCLUDED."STATOMERCE",
                            "AEROPORTODEST" = EXCLUDED."AEROPORTODEST",
                            "QT_XRAY"       = EXCLUDED."QT_XRAY",
                            "QT_ETD"        = EXCLUDED."QT_ETD",
                            "QT_PHS"        = EXCLUDED."QT_PHS",
                            "QT_VCK"        = EXCLUDED."QT_VCK"
                        RETURNING "ID","PROGRESSIVO";
                    
                    """;
                    }

                    result = conn.QuerySingle<DBResult>(sqlCommand, modelloXray);
                    return result;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento nel database.\nErrore : {ex.Message}";
                    result.ID = -1;
                    return result;
                }
            }

        }
        #endregion

        #region "Metodo per aggiornare un record nel database"
        public static int UpdateSicurezzaDB(ModelloXray modelloXray)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;

                    if (isProd)
                    {
                        sqlCommand = """
                    
                        UPDATE sicurezze."SicurRegistroSicurezze" SET "PROGRESSIVO" = @Progressivo, "NRENTRATA" = @NrEntrata, "CODICEENAC" = @CodiceEnac, "AWB" = @Awb,
                        "COLLI" = @Colli, "PESO" = @Peso, "DESTINAZIONE" = @Destinazione, "CONTENUTO" = @Contenuto, "CLIENTE" =@Cliente,
                        "OPERATORE" = @Operatore, "DATAESECUZIONE" = @DataEsecuzione, "RIFERIMENTO" = @Riferimento, "TRASPORTATORE" = @Trasportatore,
                        "TARGA" = @Targa, "SIGILLONUMERO" = @SigilloNumero, "AUTISTA" = @Autista, "XRAY" = @XRAY, "ETD" = @ETD, "PHS" = @PHS, "VCK" = @VCK,
                        "STATOMERCE" = @STATOMERCE, "AEROPORTODEST" = @AeroportoDest,
                        "QT_XRAY" = @QT_XRAY, "QT_ETD" = @QT_ETD, "QT_PHS" = @QT_PHS, "QT_VCK" = @QT_VCK
                        WHERE "ID" = @ID
                    
                        """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                        UPDATE mezzapesa."SicurRegistroSicurezze" SET "PROGRESSIVO" = @Progressivo, "NRENTRATA" = @NrEntrata, "CODICEENAC" = @CodiceEnac, "AWB" = @Awb,
                        "COLLI" = @Colli, "PESO" = @Peso, "DESTINAZIONE" = @Destinazione, "CONTENUTO" = @Contenuto, "CLIENTE" =@Cliente,
                        "OPERATORE" = @Operatore, "DATAESECUZIONE" = @DataEsecuzione, "RIFERIMENTO" = @Riferimento, "TRASPORTATORE" = @Trasportatore,
                        "TARGA" = @Targa, "SIGILLONUMERO" = @SigilloNumero, "AUTISTA" = @Autista, "XRAY" = @XRAY, "ETD" = @ETD, "PHS" = @PHS, "VCK" = @VCK,
                        "STATOMERCE" = @STATOMERCE, "AEROPORTODEST" = @AeroportoDest,
                        "QT_XRAY" = @QT_XRAY, "QT_ETD" = @QT_ETD, "QT_PHS" = @QT_PHS, "QT_VCK" = @QT_VCK
                        WHERE "ID" = @ID
                    
                        """;
                    }

                    int righeAggiornate = conn.Execute(sqlCommand, modelloXray);
                    return righeAggiornate;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento nel database.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion

        #region "Metodo per selezionare i primi 10 record"
        public static List<ModelloXray> SelectTop10(string operatore)
        {
            List<ModelloXray> lastinsert = new();
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                        SELECT *
                        FROM sicurezze."SicurRegistroSicurezze" 
                        where "OPERATORE" = @Operatore order by "ID" desc
                        limit 10
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                        SELECT *
                        FROM mezzapesa."SicurRegistroSicurezze" 
                        where "OPERATORE" = @Operatore order by "ID" desc
                        limit 10
                    
                    """;
                    }

                    return conn.Query<ModelloXray>(sqlCommand, new { Operatore = operatore }).ToList();

                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento nel database.\nErrore : {ex.Message}";
                    return lastinsert;
                }
            }
        }
        #endregion

        #region "Metodo per selezionare i codici Enac"
        public static IReadOnlyList<CodiciEnac> CodiciEnac()
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                        SELECT * FROM sicurezze."SicurCodEnac" order by "Codice_EU" asc
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                        SELECT * FROM mezzapesa."SicurCodEnac" order by "Codice_EU" asc
                    
                    """;
                    }
                    return [.. conn.Query<CodiciEnac>(sqlCommand)];
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving Codici Enac: " + ex.Message);
                }
            }
        }
        #endregion

        #region "Metodo per selezionare i codici Operatori"
        public static IReadOnlyList<CodiciOperatori> CodiciOperatori()
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                        SELECT * FROM sicurezze."SicurCodOperatore" order by "CODICE_OPERATORE" asc
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                        SELECT * FROM mezzapesa."SicurCodOperatore" order by "CODICE_OPERATORE" asc
                    
                    """;
                    }
                    return [.. conn.Query<CodiciOperatori>(sqlCommand)];
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving Codici Operatore: " + ex.Message);
                }
            }
        }
        #endregion

        #region "Metodo per selezionare i nominativi dei clienti"
        public static IReadOnlyList<SicurClienteCliente> NominativiClienti()
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = "SELECT * FROM sicurezze.\"SicurCliente\" order by \"Cliente\" asc";
                    }
                    else
                    {
                        sqlCommand = "SELECT * FROM mezzapesa.\"SicurCliente\" order by \"Cliente\" asc";
                    }
                    return [.. conn.Query<SicurClienteCliente>(sqlCommand)];
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving Nominativi Clienti: " + ex.Message);
                }
            }
        }
        #endregion

        #region "Metodo per inserire un nuovo operatore nel database"
        public static int InsertNewOperator(CodiciOperatori codiceOperatore)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    INSERT INTO sicurezze."SicurCodOperatore" ("CODICE_OPERATORE", "NOME_COGNOME", "SEDE")
                    VALUES (@Codice_Operatore, @Nome_Cognome, @Sede)
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    INSERT INTO mezzapesa."SicurCodOperatore" ("CODICE_OPERATORE", "NOME_COGNOME", "SEDE")
                    VALUES (@Codice_Operatore, @Nome_Cognome, @Sede)
                    
                    """;
                    }

                    int righeInserite = conn.ExecuteScalar<int>(sqlCommand, codiceOperatore);
                    return righeInserite;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento del nuovo operatore nel database.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion

        #region "Metodo per verificare se esiste un operatore con lo stesso codice"
        public static bool OperatoreEsistente(int codiceOperatore)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = "SELECT COUNT(*) FROM sicurezze.\"SicurCodOperatore\" WHERE \"CODICE_OPERATORE\" = @Codice_Operatore";
                    }
                    else
                    {
                        sqlCommand = "SELECT COUNT(*) FROM mezzapesa.\"SicurCodOperatore\" WHERE \"CODICE_OPERATORE\" = @Codice_Operatore";
                    }

                    int count = conn.ExecuteScalar<int>(sqlCommand, new { Codice_Operatore = codiceOperatore });
                    return count > 0;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante la verifica dell'esistenza dell'operatore.\nErrore : {ex.Message}";
                    return false;
                }
            }
        }
        #endregion

        #region "Metodo per verificare se esiste un cliente con lo stesso nominativo"
        public static bool ClienteEsistente(string Cliente)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = "SELECT COUNT(*) FROM sicurezze.\"SicurCliente\" WHERE \"Cliente\" = @cliente";
                    }
                    else
                    {
                        sqlCommand = "SELECT COUNT(*) FROM mezzapesa.\"SicurCliente\" WHERE \"Cliente\" = @cliente";
                    }

                    int count = conn.ExecuteScalar<int>(sqlCommand, new { cliente = Cliente });
                    return count > 0;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante la verifica dell'esistenza del Cliente.\nErrore : {ex.Message}";
                    return false;
                }
            }
        }
        #endregion

        #region "Metodo per inserire un nuovo cliente nel database"
        public static int InsertNewCliente(SicurClienteCliente sicurCliente)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    INSERT INTO sicurezze."SicurCliente"("Cliente")
                    VALUES (@Cliente)
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    INSERT INTO mezzapesa."SicurCliente"("Cliente")
                    VALUES (@Cliente)
                    
                    """;
                    }

                    int righeInserite = conn.ExecuteScalar<int>(sqlCommand, sicurCliente);
                    return righeInserite;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento del nuovo cliente nel database.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion

        #region "Metodo per cancellare una sicurezza dal database"
        public static int DeleteSicurezza(int ID)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = "DELETE FROM sicurezze.\"SicurRegistroSicurezze\" WHERE \"ID\" = @ID";
                    }
                    else
                    {
                        sqlCommand = "DELETE FROM mezzapesa.\"SicurRegistroSicurezze\" WHERE \"ID\" = @ID";
                    }


                    int righeCancellate = conn.Execute(sqlCommand, new { ID });
                    return righeCancellate;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante la cancellazione della sicurezza.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion

        #region "Metodo per recuperare una sicurezza dal database tramite AWB"
        public static ModelloXray GetSicurezzaByAWB(string sicurezza)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    messageError = string.Empty;
                    conn.Open();
                    string sqlCommand;

                    if (isProd)
                    {
                        sqlCommand = """
                    
                    SELECT *
                    FROM sicurezze."SicurRegistroSicurezze" 
                    WHERE "AWB" = @sicurezza
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    SELECT *
                    FROM mezzapesa."SicurRegistroSicurezze" 
                    WHERE "AWB" = @sicurezza
                    
                    """;
                    }

                    return conn.QueryFirstOrDefault<ModelloXray>(sqlCommand, new { sicurezza });
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero della sicurezza.\nErrore : {ex.Message}";
                    return null;
                }
            }

        }
        #endregion

        #region "Metodo per salvare un sigillo nel database"
        public static int SalvaSigillo(Sigillo sigillo)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    int result;

                    if (isProd)
                    {
                        result = conn.Execute("CALL sicurezze.inserisci_sigillo(@NRSIGILLO,@DESTINAZIONE,@TARGA,@TRASPORTATORE)", sigillo);
                    }
                    else
                    {
                        result = conn.Execute("CALL mezzapesa.inserisci_sigillo(@NRSIGILLO,@DESTINAZIONE,@TARGA,@TRASPORTATORE)", sigillo);
                    }

                    if (result == -1)
                    {
                        messageError = "Sigillo salvato correttamente.";
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il salvataggio del sigillo.\nErrore : {ex.Message}";
                    return -2;
                }
            }
        }
        #endregion

        #region "Metodo per recuperare tutti i sigilli dal database"
        public static List<Sigillo> GetAllSigilli()
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    List<Sigillo> sigilli = new List<Sigillo>();
                    messageError = string.Empty;

                    if (isProd)
                    {
                        sigilli = conn.Query<Sigillo>("SELECT * FROM sicurezze.\"RegistroSigilli\" ORDER BY \"DATAINSERIMENTO\" DESC", new { }).ToList();
                    }
                    else
                    {
                        sigilli = conn.Query<Sigillo>("SELECT * FROM mezzapesa.\"RegistroSigilli\" ORDER BY \"DATAINSERIMENTO\" DESC").ToList();
                    }

                    return sigilli;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero degli ultimi sigilli inseriti.\nErrore : {ex.Message}";
                    return null;
                }
            }
        }
        #endregion

        #region "Metodo per cercare i sigilli nel database in base a un termine di ricerca e un campo specifico"
        public static List<Sigillo> SearchByString(string searchTerm, string searchField)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    List<Sigillo> sigilli = new List<Sigillo>();
                    messageError = string.Empty;

                    if (isProd)
                    {
                        sigilli = conn.Query<Sigillo>($"SELECT * FROM sicurezze.\"RegistroSigilli\" WHERE \"{searchField}\" LIKE @searchTerm ORDER BY \"DATAINSERIMENTO\" DESC", new { searchTerm = $"%{searchTerm}%" }).ToList();
                    }
                    else
                    {
                        sigilli = conn.Query<Sigillo>($"SELECT * FROM mezzapesa.\"RegistroSigilli\" WHERE \"{searchField}\" LIKE @searchTerm ORDER BY \"DATAINSERIMENTO\" DESC", new { searchTerm = $"%{searchTerm}%" }).ToList();
                    }

                    return sigilli;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante la ricerca dei sigilli.\nErrore : {ex.Message}";
                    return null;
                }
            }
        }
        #endregion

        #region "Metodo per recuperare una lista di sicurezze dal database tramite AWB"
        public static List<ModelloXray> GetListSicurezzeByAWB(string sicurezza)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    messageError = string.Empty;
                    conn.Open();
                    string sqlCommand;

                    if (isProd)
                    {
                        sqlCommand = """
                    
                    SELECT *
                    FROM sicurezze."SicurRegistroSicurezze" 
                    WHERE "AWB" = @sicurezza
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    SELECT *
                    FROM mezzapesa."SicurRegistroSicurezze" 
                    WHERE "AWB" = @sicurezza
                    
                    """;
                    }

                    return [.. conn.Query<ModelloXray>(sqlCommand, new { sicurezza })];
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero della sicurezza.\nErrore : {ex.Message}";
                    return null;
                }
            }

        }
        #endregion

        #region "Metodo per recuperare una sicurezza dal database tramite AWB e ID"
        public static ModelloXray GetSicurezzaByAWBandID(string sicurezza, int id)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    messageError = string.Empty;
                    conn.Open();
                    string sqlCommand;

                    if (isProd)
                    {
                        sqlCommand = """
                    
                    SELECT *
                    FROM sicurezze."SicurRegistroSicurezze" 
                    WHERE "AWB" = @sicurezza AND "ID" = @id
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    SELECT *
                    FROM mezzapesa."SicurRegistroSicurezze" 
                    WHERE "AWB" = @sicurezza AND "ID" = @id
                    
                    """;
                    }

                    return conn.QueryFirstOrDefault<ModelloXray>(sqlCommand, new { sicurezza, id });
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero della sicurezza.\nErrore : {ex.Message}";
                    return null;
                }
            }

        }
        #endregion

        #region "Metodo per inserire la lista di autisti/targhe/ragioni sociali"
        public static int InsertNewAutistaTarga(AutistaTarga autistaTarga)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    INSERT INTO sicurezze."SicurAutistaTarga" ("Autista", "Targa", "RagioneSociale")
                    VALUES (@Autista, @Targa, @RagioneSociale)
                    RETURNING "ID"
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    INSERT INTO mezzapesa."SicurAutistaTarga" ("Autista", "Targa", "RagioneSociale")
                    VALUES (@Autista, @Targa, @RagioneSociale)
                    RETURNING "ID"
                    
                    """;
                    }

                    int righeInserite = conn.ExecuteScalar<int>(sqlCommand, autistaTarga);
                    return righeInserite;
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'inserimento dell'autista/targa/ragione sociale.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion

        #region "Metodo per recuperare la lista di autisti/targhe/ragioni sociali"
        public static List<AutistaTarga> GetAutistiTargheRagioniSociali()
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    SELECT * FROM sicurezze."SicurAutistaTarga" ORDER BY "RagioneSociale" ASC, "Autista" ASC
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    SELECT * FROM mezzapesa."SicurAutistaTarga" ORDER BY "RagioneSociale" ASC, "Autista" ASC
                    
                    """;
                    }

                    return conn.Query<AutistaTarga>(sqlCommand).ToList();
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero degli autisti/targhe/ragioni sociali.\nErrore : {ex.Message}";
                    return null;
                }
            }
        }
        #endregion

        #region "Metodo per recuperare un singolo autista/targa/ragione sociale"
        public static AutistaTarga GetAutistaTarga(int ID)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    SELECT * FROM sicurezze."SicurAutistaTarga" WHERE "ID" = @ID
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    SELECT * FROM mezzapesa."SicurAutistaTarga " WHERE "ID" = @ID
                    
                    """;
                    }

                    return conn.QueryFirstOrDefault<AutistaTarga>(sqlCommand, new { ID });
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero degli autisti/targhe/ragioni sociali.\nErrore : {ex.Message}";
                    return null;
                }
            }
        }
        #endregion

        #region "Metodo per aggiornare un singolo autista/targa/ragione sociale"
        public static int UpadateAutistaTarga(AutistaTarga autistaTarga)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    UPDATE sicurezze."SicurAutistaTarga"
                    SET "Autista" = @Autista, "Targa" = @Targa, "RagioneSociale" = @RagioneSociale
                    WHERE "ID" = @ID
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    UPDATE mezzapesa."SicurAutistaTarga"
                    SET "Autista" = @Autista, "Targa" = @Targa, "RagioneSociale" = @RagioneSociale
                    WHERE "ID" = @ID
                    
                    """;
                    }

                    return conn.Execute(sqlCommand, autistaTarga);                    
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante l'aggiornamento di autisti/targhe/ragioni sociali.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion

        #region "Metodo per cancellare un singolo autista/targa/ragione sociale"
        public static int DeleteAutistaTarga(int ID)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    DELETE FROM sicurezze."SicurAutistaTarga"
                            WHERE "ID" = @ID;                   
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    DELETE FROM mezzapesa."SicurAutistaTarga"
                            WHERE "ID" = @ID;
                    
                    """;
                    }

                    return conn.Execute(sqlCommand, new { ID = ID });
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante la cancellazione di autisti/targhe/ragioni sociali.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion

        #region "Metodo per controllora se una targa è già presente"
        public static int VerificaTarga(string targa)
        {
            using (IDbConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    string sqlCommand;
                    if (isProd)
                    {
                        sqlCommand = """
                    
                    SELECT COUNT(*) FROM sicurezze."SicurAutistaTarga" WHERE "Targa" = @Targa
                    
                    """;
                    }
                    else
                    {
                        sqlCommand = """
                    
                    SELECT COUNT(*) FROM mezzapesa."SicurAutistaTarga" WHERE "Targa" = @Targa
                    
                    
                    """;
                    }

                    return conn.QueryFirstOrDefault<int>(sqlCommand, new { Targa = targa });
                }
                catch (Exception ex)
                {
                    messageError = $"Errore durante il recupero degli autisti/targhe/ragioni sociali.\nErrore : {ex.Message}";
                    return -1;
                }
            }
        }
        #endregion        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ProyectoFinalAmaury
{
    class AnaLex
    {
        string[] palabrasReservadas =
        {
            "PROGRAMA",
            "FINPROG",
            "SI",
            "ENTONCES",
            "SINO",
            "FINSI",
            "REPITE",
            "VECES",
            "FINREP",
            "IMPRIME",
            "LEE",
        };

        string[] operadoresRelacionales =
        {
            ">", //Mayor Que
            "<", //Menor Que
            "==", //Igual a
            "=", //Asignacion
            "#", //Comentario
        };

        string[] operadoresAritmeticos =
        {
            "+", //Suma
            "-", //Resta
            "*", //Multiplicacion
            "/", //Division
        };

        string[] parseResultado;
        List<string> tokens = new List<string>();
        List<int> tokensLineas = new List<int>(); //Con esto se guardan los numeros de linea
        public List<int> tablaLineas = new List<int>(); //Con esto se sabe el numero de linea de cada elemento
        List<string> tablaIDS = new List<string>(); 
        List<string> tablaTXT = new List<string>(); 
        List<string> tablaVAL = new List<string>(); 
        List<string> lexFinal = new List<string>(); 
        List<string> simFinal = new List<string>(); 

        List<string> idsRepetidos = new List<string>();
        List<string> txtRepetidos = new List<string>();
        List<string> valRepetidos = new List<string>();

        string[] lineaTemp;
        string lineaActual;
        bool deteccionPalabras = false;
        bool verTipo = false;
        bool esVariable = false;
        bool esNumero = false;
        bool esCadena = false;

        string cadenaActual;
        bool cadenaEmpezada = false;

        bool ignorarLinea = false;
        bool ignorarEspacio = false;

        OutputLogger Logger;
        public string lexArchivoPath;
        public string simArchivoPath;

        bool yaCorrio = false;

        public void Analizar(string path, OutputLogger logger)
        {
            if (yaCorrio)
            {
                ResetearValores();
            }
            Logger = logger;
            Logger.LogLexico("CARGAR ARCHIVO CODIGO: ( "+path+" )");

            if (File.Exists(path))
            {
                parseResultado = File.ReadAllLines(path);
            }

            Logger.LogLexico("============================================");
            Logger.LogLexico("LEER CODIGO");

            for (int i = 0; i < parseResultado.Length; i++)
            {
                Logger.LogLexico("[LEER LINEA "+(i+1)+"] "+parseResultado[i]);
            }

            Logger.LogLexico("============================================");
            Logger.LogLexico("ANALIZAR LEXICO");

            tokens.Clear();
            for (int i = 0; i < parseResultado.Length; i++)
            {
                ignorarLinea = false;
                lineaActual = parseResultado[i].Trim();
                IgnorarComentarios(i);
                if (!ignorarLinea)
                {
                    lineaTemp = Regex.Split(lineaActual, "( )");
                    for (int j = 0; j < lineaTemp.Length; j++)
                    {
                        ignorarEspacio = false;
                        Logger.LogLexico("[LINEA " + (i + 1) + "] |" + lineaTemp[j] + "|");
                        IgnorarVacios(j);
                        if (!ignorarEspacio)
                        {
                            verTipo = false;
                            if (cadenaEmpezada) Logger.LogLexico("YA HAY CADENA EMPEZADA");
                            deteccionPalabras = EncontrarPalabrasReservadas(j, i);
                            if (!deteccionPalabras) { deteccionPalabras = EncontrarOperadoresRelacionales(j, i); }
                            if (!deteccionPalabras) { deteccionPalabras = EncontrarOperadoresAritmeticos(j, i); }
                            if (!deteccionPalabras) { verTipo = true; }
                            if (verTipo)
                            {
                                esCadena = DetectarCadena(j, i);
                                if (!esCadena)
                                {
                                    esNumero = DetectarNumero(j, i);
                                    if (!esNumero)
                                    {
                                        esVariable = DetectarVariable(j, i);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Logger.LogLexico("============================================");
            Logger.LogLexico("TABLA TOKENS");

            for (int i = 0; i < tokens.Count; i++)
            {
                Logger.LogLexico("TOKEN ["+(i+1)+"] "+tokens[i]+" (LINEA "+tokensLineas[i]+")");
            }

            Logger.LogLexico("============================================");
            Logger.LogLexico("GENERAR LEXICO");

            TransformarTablaTokens_Lex();
            TransformarTablaTokens_Sim();

            for (int i = 0; i < lexFinal.Count; i++)
            {
                Logger.LogLexico("LEX_FINAL ["+(i+1)+"] "+lexFinal[i]+" (LINEA "+tablaLineas[i]+")");
            }

            Logger.LogLexico("CREANDO ARCHIVO .LEX");
            lexArchivoPath = CrearArchivoDotLex(path);

            Logger.LogLexico("CREANDO ARCHIVO .SIM");
            simArchivoPath = CrearArchivoDotSim(path);

            Logger.LogLexico("ARCHIVOS .LEX Y .SIM CREADOS CON EXITO");

            yaCorrio = true;
        }

        private void IgnorarComentarios(int indice)
        {
            //Tratar como un solo token a los comentarios
            if (lineaActual.StartsWith("#"))
            {
                //tokens.Add(lineaActual);
                ignorarLinea = true;
                Logger.LogLexico("[SALTANDOSE LINEA]");
            }
            else if (string.IsNullOrEmpty(lineaActual) || string.IsNullOrWhiteSpace(lineaActual) || lineaActual == " ")
            {
                ignorarLinea = true;
                Logger.LogLexico("[SALTANDOSE LINEA]");
            }
        }

        private void IgnorarVacios(int indice)
        {
            //Ignorar los espacios vacios entre palabras que no sean cadena
            if (lineaTemp[indice] == " " && !cadenaEmpezada)
            {
                ignorarEspacio = true;
            }
        }

        private bool EncontrarPalabrasReservadas(int indice, int numLinea)
        {
            for (int i = 0; i < palabrasReservadas.Length; i++)
            {
                if (lineaTemp[indice] == palabrasReservadas[i])
                {
                    if (!cadenaEmpezada)
                    {
                        tokens.Add(lineaTemp[indice]);
                        tokensLineas.Add(numLinea+1);
                    }
                    else
                    {
                        cadenaActual += lineaTemp[indice];
                        Logger.LogLexico("CADENA ACTUAL: |" + cadenaActual + "|");
                    }
                    Logger.LogLexico("ES PALABRA RESERVADA");
                    return true;
                }
            }
            return false;
        }

        private bool EncontrarOperadoresRelacionales(int indice, int numLinea)
        {
            for (int i = 0; i < operadoresRelacionales.Length; i++)
            {
                if (lineaTemp[indice] == operadoresRelacionales[i])
                {
                    if (!cadenaEmpezada)
                    {
                        tokens.Add(lineaTemp[indice]);
                        tokensLineas.Add(numLinea+1);
                    }
                    else
                    {
                        cadenaActual += lineaTemp[indice];
                        Logger.LogLexico("CADENA ACTUAL: |" + cadenaActual + "|");
                    }
                    Logger.LogLexico("ES OPERADOR RELACIONAL");
                    return true;
                }
            }
            return false;
        }

        private bool EncontrarOperadoresAritmeticos(int indice, int numLinea)
        {
            for (int i = 0; i < operadoresAritmeticos.Length; i++)
            {
                if (lineaTemp[indice] == operadoresAritmeticos[i])
                {
                    if (!cadenaEmpezada)
                    {
                        tokens.Add(lineaTemp[indice]);
                        tokensLineas.Add(numLinea+1);
                    }
                    else
                    {
                        cadenaActual += lineaTemp[indice];
                        Logger.LogLexico("CADENA ACTUAL: |" + cadenaActual + "|");
                    }
                    Logger.LogLexico("ES OPERADOR ARITMETICO");
                    return true;
                }
            }
            return false;
        }

        private bool DetectarCadena(int indice, int numLinea)
        {
            var debouncer = false;
            var posibleSinEspacios = false;

            if (cadenaEmpezada == false && lineaTemp[indice].StartsWith("\"") && debouncer == false)
            {
                debouncer = true;
                cadenaEmpezada = true;
                cadenaActual += lineaTemp[indice];
                Logger.LogLexico("CADENA EMPEZADA: |" + cadenaActual + "|");
                if (lineaTemp[indice].EndsWith("\"") && lineaTemp[indice].Length > 1) //hay que tener cuidado que no reconozca el " inicial como el final
                { posibleSinEspacios = true; } //puede que la cadena termine en la misma palabra
            }

            if (cadenaEmpezada == true && lineaTemp[indice].EndsWith("\"") && debouncer == false || posibleSinEspacios)
            {
                debouncer = true;
                cadenaEmpezada = false;
                if (posibleSinEspacios == false)
                {
                    cadenaActual += lineaTemp[indice];
                }
                Logger.LogLexico("CADENA TERMINADA: |" + cadenaActual + "|");
                tokens.Add(cadenaActual);
                tokensLineas.Add(numLinea+1);

                //Una cadena repetida cuenta como una cadena diferente o como la misma?
                //Verificar si ya existe la cadena
                bool yaExiste = false;
                for (int i = 0; i < tablaTXT.Count; i++)
                {
                    if (tablaTXT[i] == cadenaActual)
                    {
                        Logger.LogLexico("ESTA VARIABLE YA EXISTE EN NUESTRA MEMORIA (" + tablaTXT[i] + ") [TX" + (i + 1) + "]");
                        yaExiste = true;
                    }
                }

                if (!yaExiste) { tablaTXT.Add(cadenaActual); }

                cadenaActual = "";
            }

            if (cadenaEmpezada == true && debouncer == false)
            {
                debouncer = true;
                cadenaActual += lineaTemp[indice];
                Logger.LogLexico("CADENA ACTUAL: |" + cadenaActual + "|");
            }

            if (debouncer == true) return true;
            return false;
        }

        private bool DetectarNumero(int indice, int numLinea)
        {
            var esNumero = false;
            if (cadenaEmpezada == false)
            {
                esNumero = int.TryParse(lineaTemp[indice], out int n);
            }
            if (esNumero)
            {
                tokens.Add(lineaTemp[indice]);
                tokensLineas.Add(numLinea+1);
                Logger.LogLexico("ES NUMERO");

                //Un valor repetido cuenta como un valor diferente o como el mismo valor?
                //Verificar si ya existe el valor
                bool yaExiste = false;
                for (int i = 0; i < tablaVAL.Count; i++)
                {
                    if (tablaVAL[i] == lineaTemp[indice])
                    {
                        Logger.LogLexico("ESTE VALOR YA EXISTE EN NUESTRA MEMORIA ("+tablaVAL[i]+")");
                        yaExiste = true;
                    }
                }

                if (!yaExiste) { tablaVAL.Add(lineaTemp[indice]); }
            }
            return esNumero;
        }

        private bool DetectarVariable(int indice, int numLinea)
        {
            if (cadenaEmpezada == false)
            {
                tokens.Add(lineaTemp[indice]);
                tokensLineas.Add(numLinea+1);
                Logger.LogLexico("ES VARIABLE");
            }

            //Verificar si ya existe el identificador
            bool yaExiste = false;
            for (int i = 0; i < tablaIDS.Count; i++)
            {
                if (tablaIDS[i] == lineaTemp[indice])
                {
                    Logger.LogLexico("ESTA VARIABLE YA EXISTE EN NUESTRA MEMORIA (" + tablaIDS[i] + ") [ID" + (i + 1) + "]");
                    yaExiste = true;
                }
            }

            if (!yaExiste) { tablaIDS.Add(lineaTemp[indice]); }

            return true;
        }

        private void TransformarTablaTokens_Lex()
        {
            bool forzarSiguiente = false;
            //Ahora que tenemos todos los tokens, hay que transformarlos a lexico
            for (int i = 0; i < tokens.Count; i++)
            {
                forzarSiguiente = false;
                //Si el token es un ID
                for (int j = 0; j < tablaIDS.Count; j++)
                {
                    if (tokens[i] == tablaIDS[j] && !forzarSiguiente)
                    {
                        if ((j + 1) < 10)
                        { lexFinal.Add("[id] ID0" + (j + 1)); }
                        else { lexFinal.Add("[id] ID" + (j + 1)); }
                        tablaLineas.Add(tokensLineas[i]);
                        forzarSiguiente = true;
                    }
                }

                //Si el token es una cadena
                for (int j = 0; j < tablaTXT.Count; j++)
                {
                    if (tokens[i] == tablaTXT[j] && !forzarSiguiente)
                    {
                        if ((j + 1) < 10)
                        { lexFinal.Add("[txt] TX0" + (j + 1)); }
                        else { lexFinal.Add("[txt] TX" + (j + 1)); }
                        tablaLineas.Add(tokensLineas[i]);
                        forzarSiguiente = true;
                    }
                }

                //Si el token es un valor
                for (int j = 0; j < tablaVAL.Count; j++)
                {
                    if (tokens[i] == tablaVAL[j] && !forzarSiguiente)
                    {
                        lexFinal.Add("[val]");
                        tablaLineas.Add(tokensLineas[i]);
                        forzarSiguiente = true;
                    }
                }

                //Si el token es una palabra reservada
                for (int j = 0; j < palabrasReservadas.Length; j++)
                {
                    if (tokens[i] == palabrasReservadas[j] && !forzarSiguiente)
                    {
                        lexFinal.Add(tokens[i]);
                        tablaLineas.Add(tokensLineas[i]);
                        forzarSiguiente = true;
                    }
                }

                //Si el token es un operador aritmetico
                for (int j = 0; j < operadoresAritmeticos.Length; j++)
                {
                    if (tokens[i] == operadoresAritmeticos[j] && !forzarSiguiente)
                    {
                        lexFinal.Add("[op_ar]");
                        tablaLineas.Add(tokensLineas[i]);
                        forzarSiguiente = true;
                    }
                }

                //Si el token es un operador relacional
                for (int j = 0; j < operadoresRelacionales.Length; j++)
                {
                    if (tokens[i] == operadoresRelacionales[j] && !forzarSiguiente)
                    {
                        if (tokens[i] == "=")
                        {
                            lexFinal.Add(tokens[i]);
                        }
                        else
                        {
                            lexFinal.Add("[op_rel]");
                        }
                        tablaLineas.Add(tokensLineas[i]);
                        forzarSiguiente = true;
                    }
                }
            }
        }

        private void TransformarTablaTokens_Sim()
        {
            //Ahora que tenemos todos los tokens, hay que transformarlos a lexico, pero en el archivo sim

            //en el archivo lex esto no es un problema, pero en el archivo sim los identificadores solo aparecen 1 vez.
            idsRepetidos.Clear();
            txtRepetidos.Clear();
            valRepetidos.Clear();

            //Primero los IDS
            simFinal.Add("IDS");
            for (int i = 0; i < tokens.Count; i++)
            {
                //Si el token es un ID
                for (int j = 0; j < tablaIDS.Count; j++)
                {
                    if (tokens[i] == tablaIDS[j])
                    {
                        //Verificar si no existe ya en el archivo
                        var noRepetir = false;
                        for (int k = 0; k < idsRepetidos.Count; k++)
                        {
                            if (idsRepetidos[k] == tokens[i])
                            {
                                noRepetir = true;
                            }
                        }

                        if (!noRepetir || idsRepetidos.Count == 0)
                        {
                            if ((j + 1) < 10)
                            { simFinal.Add(tokens[i] + ",ID0" + (j + 1)); }
                            else { simFinal.Add(tokens[i] + ",ID" + (j + 1)); }
                            idsRepetidos.Add(tokens[i]);
                        }
                    }
                }
            }

            //Segundo los TXT
            simFinal.Add(" ");
            simFinal.Add("TXT");
            for (int i = 0; i < tokens.Count; i++)
            {
                //Si el token es una cadena
                for (int j = 0; j < tablaTXT.Count; j++)
                {
                    if (tokens[i] == tablaTXT[j])
                    {
                        //Verificar si no existe ya en el archivo
                        var noRepetir = false;
                        for (int k = 0; k < txtRepetidos.Count; k++)
                        {
                            if (txtRepetidos[k] == tokens[i])
                            {
                                noRepetir = true;
                            }
                        }

                        if (!noRepetir || txtRepetidos.Count == 0)
                        {
                            if ((j + 1) < 10)
                            { simFinal.Add(tokens[i] + ",TX0" + (j + 1)); }
                            else { simFinal.Add(tokens[i] + ",TX" + (j + 1)); }
                            txtRepetidos.Add(tokens[i]);
                        }
                    }
                }
            }

            //Tercero los Valores
            simFinal.Add(" ");
            simFinal.Add("VAL");
            for (int i = 0; i < tokens.Count; i++)
            {
                //Si el token es un valor
                for (int j = 0; j < tablaVAL.Count; j++)
                {
                    if (tokens[i] == tablaVAL[j])
                    {
                        //Verificar si no existe ya en el archivo
                        var noRepetir = false;
                        for (int k = 0; k < valRepetidos.Count; k++)
                        {
                            if (valRepetidos[k] == tokens[i])
                            {
                                noRepetir = true;
                            }
                        }

                        if (!noRepetir || valRepetidos.Count == 0)
                        {
                            int val;
                            int.TryParse(tokens[i], out val);
                            simFinal.Add(Convert.ToString(val, 8)+","+tokens[i]);
                            valRepetidos.Add(tokens[i]);
                        }
                    }
                }
            }
        }

        public string CrearArchivoDotLex(string pathRel)
        {
            var path = Path.GetDirectoryName(pathRel);
            File.WriteAllText(Path.GetFullPath(path + "/" + tablaIDS[0] + ".lex"), string.Empty);
            File.AppendAllLines(Path.GetFullPath(path + "/" + tablaIDS[0] + ".lex"), lexFinal);

            return path+"/"+tablaIDS[0]+".lex";
        }

        public string CrearArchivoDotSim(string pathRel)
        {
            var path = Path.GetDirectoryName(pathRel);
            File.WriteAllText(Path.GetFullPath(path + "/" + tablaIDS[0] + ".sim"), string.Empty);
            File.AppendAllLines(Path.GetFullPath(path + "/" + tablaIDS[0] + ".sim"), simFinal);

            return path+"/"+tablaIDS[0]+".sim";
        }

        public void ResetearValores()
        {
            //Esto es por si a alguien se le ocurre presionar analizar lexico dos veces o mas.
            Array.Clear(parseResultado, 0, parseResultado.Length);
            tokens.Clear();
            tokensLineas.Clear();
            tablaIDS.Clear();
            tablaTXT.Clear();
            tablaVAL.Clear();
            lexFinal.Clear();
            simFinal.Clear();

            idsRepetidos.Clear();
            txtRepetidos.Clear();
            valRepetidos.Clear();

            Array.Clear(lineaTemp, 0, lineaTemp.Length);
            lineaActual = string.Empty;
            deteccionPalabras = false;
            verTipo = false;
            esVariable = false;
            esNumero = false;
            esCadena = false;

            cadenaActual = string.Empty;
            cadenaEmpezada = false;

            ignorarLinea = false;
            ignorarEspacio = false;
        }
    }
}

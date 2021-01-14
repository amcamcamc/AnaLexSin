using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ProyectoFinalAmaury
{
    class AnaSin
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

        enum TipoComando
        {
            PalabraReservada,
            Identificador,
            Texto,
            Valor,
            OperadorRelacional,
            OperadorAritmetico,
            Asignador,
            Invalido,
        }

        string[] parseResultado;
        List<int> elemLineas = new List<int>();

        //Temporal
        bool esId = false;
        bool esTxt = false;
        bool esVal = false;
        bool esOpAr = false;
        bool esOpRel = false;
        bool esAsignacion = false;
        bool esPalabra = false;

        TipoComando TokenActual = TipoComando.Invalido;

        int lineaTemporal = 0;
        List<int> ignorarTokens = new List<int>();
        List<int> tokensAbiertos_repeticiones = new List<int>();
        List<int> tokensAbiertos_condicionales = new List<int>();
        int finreps = 0;
        int finsis = 0;
        int sinos = 0;
        int entonceses = 0;
        int vececes = 0;

        bool sintaxisValido = true;

        bool yaCorrio = false;

        OutputLogger Logger;

        public void Analizar(string lexPath, string simPath, List<int> tablaLineas, OutputLogger logger)
        {
            if (yaCorrio)
            {
                ResetearValores();
            }

            Logger = logger;
            Logger.LogSintactico("CARGAR ARCHIVO LEX: ( "+lexPath+" )");
            //Logger.LogSintactico("CARGAR ARCHIVO SIM: ( "+simPath+" )");
            elemLineas = tablaLineas;

            if (File.Exists(lexPath))
            {
                parseResultado = File.ReadAllLines(lexPath);
            }

            //Logger.LogSintactico("============================================");
            //Logger.LogSintactico("ANALIZAR LEXICO");

            for (int i = 0; i < parseResultado.Length; i++)
            {
                //Logger.LogSintactico("[LEXICO "+(i+1)+"] "+parseResultado[i]+" (LINEA "+elemLineas[i]+")");
            }
            //Logger.LogSintactico("============================================");
            sintaxisValido = Regla_SimboloInicial(lineaTemporal);

            if (sintaxisValido)
            {
                Logger.LogSintactico("[====================]");
                Logger.LogSintactico("[ COMPILACION EXITOSA] ");
                Logger.LogSintactico("[====================]");
            }

            yaCorrio = true;
        }

        private TipoComando IdentificarComando(string comando)
        {
            esId = false;
            esTxt = false;
            esVal = false;
            esOpAr = false;
            esOpRel = false;
            esAsignacion = false;
            esPalabra = false;

            esId = comando.Contains("[id]");
            esTxt = comando.Contains("[txt]");
            esVal = comando.Contains("[val]");
            esOpAr = comando.Contains("[op_ar]");
            esOpRel = comando.Contains("[op_rel]");
            esAsignacion = comando.Contains("=");
            for (int i = 0; i < palabrasReservadas.Length; i++)
            {
                if (comando.Contains(palabrasReservadas[i]))
                {
                    esPalabra = true;
                }
            }

            if (esId) { return TipoComando.Identificador; }
            if (esTxt) { return TipoComando.Texto; }
            if (esVal) { return TipoComando.Valor; }
            if (esOpAr) { return TipoComando.OperadorAritmetico; }
            if (esOpRel) { return TipoComando.OperadorRelacional; }
            if (esAsignacion) { return TipoComando.Asignador; }
            if (esPalabra) { return TipoComando.PalabraReservada; }
            return TipoComando.Invalido;
        }

        private bool AnalizarComando(TipoComando comando, string comandoCadena, int lineaParse)
        {
            var valido = true;
            if (comando == TipoComando.PalabraReservada)
            {
                switch(comandoCadena)
                {
                    case "SI":
                        valido = Regla_Condicional(lineaParse);
                        break;
                    case "ENTONCES":
                        valido = Regla_CondicionalInicio(lineaParse);
                        break;
                    case "SINO":
                        valido = Regla_CondicionalAlternativa(lineaParse);
                        break;
                    case "FINSI":
                        valido = Regla_FinCondicional(lineaParse);
                        break;
                    case "REPITE":
                        valido = Regla_Repeticion(lineaParse);
                        break;
                    case "VECES":
                        valido = Regla_RepeticionVeces(lineaParse);
                        break;
                    case "FINREP":
                        valido = Regla_FinRepeticion(lineaParse);
                        break;
                    case "IMPRIME":
                        valido = Regla_Impresion(lineaParse);
                        break;
                    case "LEE":
                        valido = Regla_Lectura(lineaParse);
                        break;
                    default:
                        break;
                }
            }

            if (comando == TipoComando.Identificador)
            {
                valido = Regla_Asignacion(lineaParse);
            }

            return valido;
        }

        bool Regla_SimboloInicial(int lineaParse)
        {
            //Verifica El Simbolo Inicial con el que empiezan los programas
            // PROGRAMA [id] (comandos) FINPROG

            var valido = true;

            //Primero: Verificar si la palabra escrita corresponde a el simbolo inicial "PROGRAMA"

            if (parseResultado[0] != "PROGRAMA" || IdentificarComando(parseResultado[0]) != TipoComando.PalabraReservada)
            { ErrorSintactico("SIMBOLO INICIAL FALTANTE", 0); return false; }

            //Segundo: Verificar si el siguiente token es un Identificador
            if (IdentificarComando(parseResultado[lineaParse+1]) != TipoComando.Identificador)
            { ErrorSintactico("NOMBRE DE PROGRAMA INCORRECTO", elemLineas[lineaParse]); return false; }

            //Logger.LogSintactico("SIMBOLO INCIAL VALIDO: PROGRAMA");

            //Tercero: Continuar Buscando Recursivamente comandos si es que existen
            for (int i = lineaParse+2; i < parseResultado.Length; i++)
            {
                //Console.WriteLine("token actual num: "+i);
                //Console.WriteLine("condicionales abiertas: " + tokensAbiertos_condicionales.Count);
                //Console.WriteLine("ciclos abiertos: " + tokensAbiertos_repeticiones.Count);
                //Console.WriteLine("===================");
                //Console.WriteLine("Tokens a ignorar");
                var ignorarToken = false;
                for (int j = 0; j < ignorarTokens.Count; j++)
                {
                    //Console.WriteLine(ignorarTokens[j]);
                    if ((ignorarTokens[j]) == i)
                    {
                        //Console.WriteLine("ignorar token " + i);
                        ignorarToken = true;
                    }
                }
                if (ignorarTokens.Count == 0 || ignorarToken == false)
                {
                    var TokenActual_PROG = IdentificarComando(parseResultado[i]);
                    //Logger.LogSintactico("TOKEN ACTUAL: " + TokenActual_PROG + " [num: "+i+"] (" + parseResultado[i] + ") (LINEA " + elemLineas[i] + ")");
                    valido = AnalizarComando(TokenActual_PROG, parseResultado[i], i);
                }
                if (!valido) { return false; }
            }

            //Cuarto: Si ya no hay sentencias, verificar que el ultimo comando sea FINPROG
            if (IdentificarComando(parseResultado[parseResultado.Length-1]) != TipoComando.PalabraReservada || parseResultado[parseResultado.Length-1] != "FINPROG")
            { ErrorSintactico("SIMBOLO FINAL FALTANTE", 0); return false; }

            //Quinto: Verificar que no hayan ciclos o condicionales abiertas sin cerrar
            if (tokensAbiertos_condicionales.Count != 0)
            { 
                ErrorSintactico("FALTAN POR CERRAR CONDICIONALES", 0);
                for (int i = 0; i < tokensAbiertos_condicionales.Count; i++)
                { Logger.LogSintactico("CONDICIONAL SIN CERRAR EN LINEA: " + elemLineas[tokensAbiertos_condicionales[i]]); }
                return false;
            } 
            if (tokensAbiertos_repeticiones.Count != 0)
            { 
                ErrorSintactico("FALTAN POR CERRAR CICLO", 0);
                for (int i = 0; i < tokensAbiertos_repeticiones.Count; i++)
                { Logger.LogSintactico("CICLO SIN CERRAR EN LINEA: " + elemLineas[tokensAbiertos_repeticiones[i]]); }
                return false; 
            }

            return true;
        }

        bool Regla_Asignacion(int lineaParse)
        {
            //Verifica si el Identificador tiene una asignacion correcta
            //[id] = (ELEM)

            var comandoExtendido = false;

            //Primero: Verificar si el simbolo siguiente es un asignador
            if (IdentificarComando(parseResultado[lineaParse + 1]) != TipoComando.Asignador)
            { ErrorSintactico("SIMBOLO ASIGNADOR FALTANTE", elemLineas[lineaParse]); return false; }

            //Segundo: Verificar si el simbolo siguiente es un Elemento (puede ser identificador o valor solamente)
            if (EsElemento(parseResultado[lineaParse + 2]) == false)
            { ErrorSintactico("NO HAY ELEMENTO PARA ASIGNAR", elemLineas[lineaParse]); return false; }

            //Tercero: Verificar si el simbolo siguiente es un operador aritmetico para aplicar esta regla derivada:
            //[id] = (ELEM) [op_ar] (ELEM)
            if (IdentificarComando(parseResultado[lineaParse + 3]) == TipoComando.OperadorAritmetico)
            {
                if (IdentificarComando(parseResultado[lineaParse + 4]) != TipoComando.Identificador && IdentificarComando(parseResultado[lineaParse + 4]) != TipoComando.Valor)
                {
                    { ErrorSintactico("ERROR ENTRE OPERACION DE ELEMENTOS EN LA ASIGNACION", elemLineas[lineaParse]); return false; }
                }
                comandoExtendido = true;
            }

            if (!comandoExtendido)
            {
                //Logger.LogSintactico("COMANDO VALIDO: " + parseResultado[lineaParse] + " " + parseResultado[lineaParse + 1] + " " + parseResultado[lineaParse + 2]);
                ignorarTokens.Add(lineaParse+1);
                ignorarTokens.Add(lineaParse+2);
            }
            else
            {
                //Logger.LogSintactico("COMANDO VALIDO: " + parseResultado[lineaParse] + " " + parseResultado[lineaParse + 1] + " " + parseResultado[lineaParse + 2] + " " + parseResultado[lineaParse + 3] + " " + parseResultado[lineaParse + 4]);
                ignorarTokens.Add(lineaParse+1);
                ignorarTokens.Add(lineaParse+2);
                ignorarTokens.Add(lineaParse+3);
                ignorarTokens.Add(lineaParse+4);
            }

            return true;
        }

        bool Regla_Lectura(int lineaParse)
        {
            //Verifica si se puede leer el siguiente token
            //LEE [id]

            //Primero: Verificar si el simbolo siguiente es un identificador
            if (IdentificarComando(parseResultado[lineaParse + 1]) != TipoComando.Identificador)
            { ErrorSintactico("SOLO SE PUEDEN LEER IDENTIFICADORES", elemLineas[lineaParse]); return false; }

            //Logger.LogSintactico("COMANDO VALIDO: " + parseResultado[lineaParse] + " " + parseResultado[lineaParse + 1]);
            ignorarTokens.Add(lineaParse+1);

            return true;
        }

        bool Regla_Impresion(int lineaParse)
        {
            //Verifica si se puede imprimir el siguiente token
            //IMPRIME (elem)
            //IMPRIME [txt]

            //Primero: Verificar si el simbolo siguiente es un elemento o un texto
            if (EsElemento(parseResultado[lineaParse + 1]) == false && IdentificarComando(parseResultado[lineaParse + 1]) != TipoComando.Texto)
            { ErrorSintactico("SOLO SE PUEDEN IMPRIMIR ELEMENTOS O TEXTOS", elemLineas[lineaParse]); return false; }

            //Logger.LogSintactico("COMANDO VALIDO: " + parseResultado[lineaParse] + " " + parseResultado[lineaParse + 1]);
            ignorarTokens.Add(lineaParse + 1);

            return true;
        }

        bool Regla_Comparacion(int lineaParse)
        {
            //Verifica si se puede comparar un identificador con un elemento
            //[id] [op_rel] (ELEM)

            //Primero: Verificar si el simbolo anterior es un SI, ya que solo se puede comparar en una condicional
            if (IdentificarComando(parseResultado[lineaParse - 1]) != TipoComando.PalabraReservada && parseResultado[lineaParse-1] == "SI")
            { ErrorSintactico("\"SI\" FALTANTE EN CONDICIONAL", elemLineas[lineaParse]); return false; }

            //Segundo: Verificar si el simbolo siguiente es un operador relacional
            if (IdentificarComando(parseResultado[lineaParse + 1]) != TipoComando.OperadorRelacional)
            { ErrorSintactico("OPERADOR RELACIONAL FALTANTE EN LA COMPARACION", elemLineas[lineaParse]); return false; }

            //Tercero: Verificar si el simbolo siguiente es un elemento
            if (EsElemento(parseResultado[lineaParse + 2]) == false)
            { ErrorSintactico("ELEMENTO FALTANTE EN LA COMPARACION", elemLineas[lineaParse]); return false; }

            //Cuarto: Verificar si el simbolo siguiente es un ENTONCES, ya que solo se puede comparar en una condicional
            if (IdentificarComando(parseResultado[lineaParse + 3]) != TipoComando.PalabraReservada || parseResultado[lineaParse + 3] != "ENTONCES")
            { ErrorSintactico("\"ENTONCES\" FALTANTE EN CONDICIONAL", elemLineas[lineaParse]); return false; }

            return true;
        }

        bool Regla_Repeticion(int lineaParse)
        {
            //Verifica la estructura correcta de una repeticion
            //REPITE (ELEM) VECES (COMANDOS) FINREP

            //Primero: Verificar si la palabra escrita corresponde a el simbolo "REPITE"
            if (parseResultado[lineaParse] != "REPITE")
            { ErrorSintactico("PALABRA \"REPITE\" FALTANTE EN CICLO", elemLineas[lineaParse]); return false; }

            //Segundo: Verificar si el simbolo siguiente es un elemento
            if (EsElemento(parseResultado[lineaParse+1]) == false)
            { ErrorSintactico("ELEMENTO FALTANTE EN CICLO (IDENTIFICADOR DE VECES)", elemLineas[lineaParse]); return false; }

            //Tercero: Verificar si la siguiente palabra escrita corresponde a el simbolo "VECES"
            if (IdentificarComando(parseResultado[lineaParse + 2]) != TipoComando.PalabraReservada || parseResultado[lineaParse + 2] != "VECES")
            { ErrorSintactico("PALABRA \"VECES\" FALTANTE EN CICLO", elemLineas[lineaParse]); return false; }

            //Tercero: Buscar Recursivamente comandos si es que existen. Para esto vamos a dejar nuestro token en una lista de
            //espera para que las demas sentencias puedan ser verificadas. Si se llega al FINPROG sin haber acabado nuestro ciclo, se lanza un error.
            tokensAbiertos_repeticiones.Add(lineaParse);
            ignorarTokens.Add(lineaParse + 1);

            return true;
        }

        bool Regla_FinRepeticion(int lineaParse)
        {
            //No hay forma de saber cual repeticion es la que se esta cerrando, asi que la unica forma es
            //quitando la ultima repeticion abierta de la lista esperando que el usuario cierre todas para que no de error.

            finreps++;
            if (tokensAbiertos_repeticiones.Count >= finreps) //creo que es la unica forma de saber si el final es correcto
            {
                tokensAbiertos_repeticiones.RemoveAt(tokensAbiertos_repeticiones.Count - 1);
                finreps--;
                if (vececes > 0) { vececes--; } //Aqui se cierran los vecess para que solo pueda haber 1 por repeticion.
                return true;
            }
            else
            { ErrorSintactico("\"FINREP\" SIN REPETIR PERTENECIENTE", elemLineas[lineaParse]); return false; }
        }

        bool Regla_RepeticionVeces(int lineaParse)
        {
            //Hay que verificar si el VECES no esta de mas.

            vececes++;
            if (tokensAbiertos_repeticiones.Count >= vececes) //se podria verificar la estructura, pero no es necesario.
            { return true; }
            else
            { ErrorSintactico("\"VECES\" SIN REPETIR PERTENECIENTE", elemLineas[lineaParse]); return false; }
        }

        bool Regla_Condicional(int lineaParse)
        {
            //Verifica la estructura correcta de una condicional
            //SI (COMPARA) ENTONCES (COMANDOS) FINSI
            //SI (COMPAR) ENTONCES (COMANDOS) SINO (COMANDOS) FINSI

            //Primero: Verificar si la palabra escrita corresponde a el simbolo "SI"

            if (parseResultado[lineaParse] != "SI")
            { ErrorSintactico("\"SI\" FALTANTE EN CONDICIONAL", elemLineas[lineaParse]); return false; }

            //Segundo: Verificar la estructura de la comparacion
            if (Regla_Comparacion(lineaParse + 1) == false)
            { ErrorSintactico("ESTRUCTURA DE COMPARACION ERRONEA EN CONDICIONAL", elemLineas[lineaParse]); return false; }

            //Tercero: Verificar si lo que sigue de la comparacion es un ENTONCES
            if (IdentificarComando(parseResultado[lineaParse + 4]) != TipoComando.PalabraReservada || parseResultado[lineaParse + 4] != "ENTONCES")
            { ErrorSintactico("\"ENTONCES\" FALTANTE EN CONDICIONAL", elemLineas[lineaParse]); return false; }

            //Cuarto: Buscar Recursivamente comandos si es que existen. Para esto vamos a dejar nuestro token en una lista de
            //espera para que las demas sentencias puedan ser verificadas. Si se llega al FINPROG sin haber acabado nuestra condicional, se lanza un error.
            tokensAbiertos_condicionales.Add(lineaParse);

            //Logger.LogSintactico("COMANDO VALIDO: " + parseResultado[lineaParse] + " " + parseResultado[lineaParse + 1] + " " + parseResultado[lineaParse + 2]);
            ignorarTokens.Add(lineaParse + 1);
            ignorarTokens.Add(lineaParse + 2);
            ignorarTokens.Add(lineaParse + 3);

            return true;
        }

        bool Regla_CondicionalAlternativa(int lineaParse)
        {
            //Aqui pense en hacer lo mismo para el SINO, pero en realidad, si se piensa bien, no es necesario verificar que exista
            //porque verificando que exista tambien quiere decir que tenemos que verificar que el SI inicial exista y que el FINSI exista
            //como ambas son recursivas, entonces no hay manera de saber exactamente si se encuentra en medio de estas.
            //Nos podriamos saltar verificar si es correcta esta palabra en la gramatica, pero decidi poner aun asi un chequeo para ver si no esta extra.

            sinos++;
            if (tokensAbiertos_condicionales.Count >= sinos) //creo que es la unica forma de saber si el sino es correcto
            { return true; }
            else
            { ErrorSintactico("\"SINO\" SIN CONDICIONAL PERTENECIENTE", elemLineas[lineaParse]); return false; }
        }

        bool Regla_CondicionalInicio(int lineaParse)
        {
            //Hay que verificar si el ENTONCES no esta de mas, al igual que el SINO

            entonceses++;
            if (tokensAbiertos_condicionales.Count >= entonceses) //se podria verificar la estructura, pero no es necesario.
            { return true; }
            else
            { ErrorSintactico("\"ENTONCES\" SIN CONDICIONAL PERTENECIENTE", elemLineas[lineaParse]); return false; }
        }

        bool Regla_FinCondicional(int lineaParse)
        {
            //No hay forma de saber cual condicion es la que se esta cerrando, asi que la unica forma es
            //quitando la ultima condicion abierta de la lista esperando que el usuario cierre todas para que no de error.
            
            finsis++;
            if (tokensAbiertos_condicionales.Count >= finsis) //creo que es la unica forma de saber si el final es correcto
            {
                tokensAbiertos_condicionales.RemoveAt(tokensAbiertos_condicionales.Count - 1);
                finsis--;
                if (sinos > 0) { sinos--; } //Aqui se cierran los sinos para que solo pueda haber 1 por condicional.
                if (entonceses > 0) { entonceses--; } //Aqui se cierran los entonces para que solo pueda haber 1 por condicional.
                return true;
            }
            else
            { ErrorSintactico("\"FINSI\" SIN CONDICIONAL PERTENECIENTE", elemLineas[lineaParse]); return false; }
        }

        bool EsElemento(string parse)
        {
            if (IdentificarComando(parse) == TipoComando.Identificador || IdentificarComando(parse) == TipoComando.Valor)
            { return true; }
            return false;
        }

        void ErrorSintactico(string razon, int linea)
        {
            Logger.LogSintactico("[====================]");
            if (linea != 0)
            { Logger.LogSintactico("[ ERROR DE COMPILACION: " + razon + " EN LA LINEA " + linea + " ]"); }
            else
            { Logger.LogSintactico("[ ERROR DE COMPILACION: " + razon +" ]"); }
            Logger.LogSintactico("[====================]");

            //Console.WriteLine("CONDICIONALES: "+tokensAbiertos_condicionales.Count +" FINSIS: "+ finsis +" SINOS: "+sinos+" ENTONCECES: " + entonceses);
            //Console.WriteLine("REPETICIONES: " + tokensAbiertos_repeticiones.Count + " FINREPS: " + finreps+" VECECES: " + vececes);
        }

        public void ResetearValores()
        {
            Array.Clear(parseResultado, 0, parseResultado.Length);
            //elemLineas.Clear();

            esId = false;
            esTxt = false;
            esVal = false;
            esOpAr = false;
            esOpRel = false;
            esAsignacion = false;
            esPalabra = false;

            TokenActual = TipoComando.Invalido;

            lineaTemporal = 0;
            ignorarTokens.Clear();
            tokensAbiertos_repeticiones.Clear();
            tokensAbiertos_condicionales.Clear();
            finreps = 0;
            finsis = 0;
            sinos = 0;
            entonceses = 0;
            vececes = 0;

            sintaxisValido = true;
        }
    }
}

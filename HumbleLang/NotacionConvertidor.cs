using System;
using System.Collections.Generic;
using System.Linq;

public static class NotacionConvertidor
{
    // Conjunto de tokens que representan operandos o literales
    private static readonly HashSet<string> Operandos = new HashSet<string>
    {
        "IDEN", "CN_ENTERA", "CN_DECIMAL", "CADE", "PR20", "PR06"
    };

    // 1.1. Definición de precedencia de operadores de HumbleLang
    // Mayor valor = Mayor precedencia
    private static int ObtenerPrecedencia(string token)
    {
        return token switch
        {
            // Operadores Lógicos (menor precedencia)
            "OPLO1" or "OPLO2" => 1,
            // Operadores Relacionales
            "OPREL1" or "OPREL2" or "OPREL3" or "OPREL4" or "OPREL5" or "OPREL6" => 2,
            // Operadores Aritméticos de Suma/Resta
            "OPRE1" or "OPRE2" => 3,
            // Operadores Aritméticos de Multiplicación/División/Módulo
            "OPRE3" or "OPRE4" or "OPRE5" => 4,
            // Operador Unario de Negación/NO
            "PR16" => 5,
            // Paréntesis de apertura, mantiene la menor precedencia dentro de la pila
            "CE6" or "CE7" => 0,
            _ => -1, // No es un operador
        };
    }

    // =========================================================================================

    // 1.2. Conversión de Infija a Postfija
    // Implementa el algoritmo Shunting-yard estándar con asociatividad de izquierda a derecha (L-to-R).
    public static List<(string Token, int Linea)> InfijaAPostfija(List<(string Token, int Linea)> tokensInfijos)
    {
        var salidaPostfija = new List<(string Token, int Linea)>();
        var pilaOperadores = new Stack<(string Token, int Linea)>();

        foreach (var tokenActual in tokensInfijos)
        {
            string tipoToken = tokenActual.Token;

            if (Operandos.Contains(tipoToken))
            {
                // 1. Si es operando, va directo a la salida.
                salidaPostfija.Add(tokenActual);
            }
            else if (tipoToken == "CE6") // Paréntesis de apertura '('
            {
                // 2. Si es '(', va a la pila.
                pilaOperadores.Push(tokenActual);
            }
            else if (tipoToken == "CE7") // Paréntesis de cierre ')'
            {
                // 3. Si es ')', desapila hasta encontrar '(' y lo descarta.
                while (pilaOperadores.Count > 0 && pilaOperadores.Peek().Token != "CE6")
                {
                    salidaPostfija.Add(pilaOperadores.Pop());
                }

                if (pilaOperadores.Count == 0 || pilaOperadores.Peek().Token != "CE6")
                {
                    throw new InvalidOperationException($"Error: Paréntesis desbalanceado en línea {tokenActual.Linea}.");
                }

                pilaOperadores.Pop(); // Descartar '('
            }
            else if (ObtenerPrecedencia(tipoToken) > 0) // Es un operador
            {
                int precActual = ObtenerPrecedencia(tipoToken);

                // 4. Desapila operadores de mayor o igual precedencia (L-to-R)
                while (pilaOperadores.Count > 0 &&
                       ObtenerPrecedencia(pilaOperadores.Peek().Token) >= precActual &&
                       pilaOperadores.Peek().Token != "PR16") // PR16 (No) es unario y generalmente R-to-L
                {
                    salidaPostfija.Add(pilaOperadores.Pop());
                }

                // 5. Colocar el operador actual en la pila
                pilaOperadores.Push(tokenActual);
            }
        }

        // 6. Mover los operadores restantes de la pila a la salida
        while (pilaOperadores.Count > 0)
        {
            var operadorRestante = pilaOperadores.Pop();
            if (operadorRestante.Token == "CE6" || operadorRestante.Token == "CE7")
            {
                throw new InvalidOperationException("Error: Paréntesis desbalanceado restante.");
            }
            salidaPostfija.Add(operadorRestante);
        }

        return salidaPostfija;
    }

    // =========================================================================================

    // 1.3. Conversión de Infija a Prefija
    // Utiliza la técnica de inversión y luego conversión a postfija.
    public static List<(string Token, int Linea)> InfijaAPrefija(List<(string Token, int Linea)> tokensInfijos)
    {
        // 1. Invertir la expresión infija
        var tokensInvertidos = tokensInfijos.AsEnumerable().Reverse().ToList();

        // 2. Reemplazar paréntesis
        var tokensParaConversion = new List<(string Token, int Linea)>();
        foreach (var token in tokensInvertidos)
        {
            string tipoToken = token.Token;
            if (tipoToken == "CE6") // ( se convierte en )
            {
                tokensParaConversion.Add(("CE7", token.Linea));
            }
            else if (tipoToken == "CE7") // ) se convierte en (
            {
                tokensParaConversion.Add(("CE6", token.Linea));
            }
            else
            {
                tokensParaConversion.Add(token);
            }
        }

        // 3. Aplicar conversión a Postfija con modificación de asociatividad
        // Nota: Al invertir y usar Shunting-yard con L-to-R, se simula el R-to-L para
        // operadores de igual precedencia, que es necesario para la notación prefija.
        var postfijaTemporal = InfijaAPostfijaParaPrefija(tokensParaConversion);

        // 4. Invertir el resultado para obtener la notación prefija
        return postfijaTemporal.AsEnumerable().Reverse().ToList();
    }

    // Método auxiliar para Infix to Prefix. Similar al de Postfija, pero con una
    // regla de precedencia modificada (asociatividad R-to-L)
    private static List<(string Token, int Linea)> InfijaAPostfijaParaPrefija(List<(string Token, int Linea)> tokensInfijos)
    {
        var salidaPostfija = new List<(string Token, int Linea)>();
        var pilaOperadores = new Stack<(string Token, int Linea)>();

        foreach (var tokenActual in tokensInfijos)
        {
            string tipoToken = tokenActual.Token;

            if (Operandos.Contains(tipoToken))
            {
                salidaPostfija.Add(tokenActual);
            }
            else if (tipoToken == "CE6")
            {
                pilaOperadores.Push(tokenActual);
            }
            else if (tipoToken == "CE7")
            {
                while (pilaOperadores.Count > 0 && pilaOperadores.Peek().Token != "CE6")
                {
                    salidaPostfija.Add(pilaOperadores.Pop());
                }

                if (pilaOperadores.Count == 0 || pilaOperadores.Peek().Token != "CE6")
                {
                    throw new InvalidOperationException($"Error: Paréntesis desbalanceado en línea {tokenActual.Linea}.");
                }

                pilaOperadores.Pop();
            }
            else if (ObtenerPrecedencia(tipoToken) > 0) // Es un operador
            {
                int precActual = ObtenerPrecedencia(tipoToken);

                // La diferencia clave:
                // Se desapila cuando la precedencia de la pila es ESTRICTAMENTE MAYOR (>),
                // respetando la asociatividad de derecha a izquierda (R-to-L) necesaria para
                // la conversión a Prefija en la expresión invertida.
                while (pilaOperadores.Count > 0 &&
                       ObtenerPrecedencia(pilaOperadores.Peek().Token) > precActual)
                {
                    salidaPostfija.Add(pilaOperadores.Pop());
                }

                pilaOperadores.Push(tokenActual);
            }
        }

        while (pilaOperadores.Count > 0)
        {
            var operadorRestante = pilaOperadores.Pop();
            if (operadorRestante.Token == "CE6" || operadorRestante.Token == "CE7")
            {
                throw new InvalidOperationException("Error: Paréntesis desbalanceado restante.");
            }
            salidaPostfija.Add(operadorRestante);
        }

        return salidaPostfija;
    }
}
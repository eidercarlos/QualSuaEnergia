using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Devemos ter um arquivo de configuração chamado settings.json que abriga as seguintes configurações:
1 - O tempo de timeout [Ou Seja, o tempo que passou sem que ninguém tenha feito alguma interação]
2 - O tempo definido de uso da tela 2 [Após esse tempo terminar é exibido a tela 3] 
3 - A url de comunicação com a API REST
4 - O caminho para salvar a imagem de acordo com o padrão do Postal Social
5 - Inverter o mouse (eixos X e Y)  
*/


public class Settings
{   
    public float timeIdleAfterStartApp;
    public float timeToPrintAfterStartInteraction;
    public float timeToGetIdle;
    public string printScrPath;
    public string printScrFileName;
    public int printScrQualityLevel;
    public string APIRestURL;
    public bool invertAxisX;
    public bool invertAxisY;
}

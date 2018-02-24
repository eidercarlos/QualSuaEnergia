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
    public float time_print_after_start_interaction;
    public float time_get_idle;
    public string print_path;
    public string print_file_name;
    public int print_quality_level;
    public string rest_api_url;
    public bool invert_axis_x;
    public bool invert_axis_y;
    public float horizontal_speed;
    public float vertical_speed;
}

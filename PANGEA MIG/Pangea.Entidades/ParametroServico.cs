using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades
{
    public class ParametroServico : EntidadeBase
    {
        public DateTime hora_inicio_religacao_automatica { get; set; }
        public DateTime hora_fim_religacao_automatica { get; set; }
        public DateTime data_atual_bd { get; set; }
        public string relig_sem_medidor { get; set; }
    }

}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Models;

[PrimaryKey(nameof(WorkspaceId), nameof(Domain))]
public class WorkspaceDomain
{
    public string WorkspaceId { get; set;  }
    public Workspace Workspace { get; set; }
    public string Domain { get; set; }
}

// File: src/modules/iam/CreditRisk.IAM.Application/Queries/GetUserById/GetUserByIdQuery.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id);

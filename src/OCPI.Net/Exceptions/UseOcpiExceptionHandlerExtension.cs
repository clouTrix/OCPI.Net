﻿using BitzArt.ApiExceptions.AspNetCore;
using Microsoft.AspNetCore.Builder;
using OCPI.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace OCPI;

public static class UseOcpiExceptionHandlerExtension
{
    public static void UseOcpiExceptionHandler(this IApplicationBuilder app)
    {
        app.UseApiExceptionHandler<OcpiExceptionHandler>();
    }
}

using AttendanceStudents.Service.Interfaces;

namespace AttendanceStudents.Service.Implementations;

using QRCoder;

public class QrCodeService : IQRCodeService
{
    public string GeneratePngDataUri(string text)
    {
        
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var bytes = qrCode.GetGraphic(20);

        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}
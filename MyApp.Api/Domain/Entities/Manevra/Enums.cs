namespace MyApp.Api.Domain.Entities.Manevra;

public enum WagonLine : byte
{
    M1 = 1,
    Tramvay = 2,
}

public enum WagonStatus : byte
{
    Servis = 1,
    CalismaYapilacak = 2,
    ServiseHazir = 3,
}

public enum TrackZone : byte
{
    Garaj = 1,
    Atolye = 2,
    CariHattaHazirDiziler = 3,
}

public enum SectionType : byte
{
    BasMakasYonu = 1,
    ItfaiyeYonu = 2,
    ItfaiyeTaraf = 3,
    AtolyeYollari = 4,
    YikamaTaraf = 5,
    HazirDiziler = 6,
}

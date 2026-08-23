namespace WorldNMilSim.Components;

public class StealthComponent
{
    public double RadarSignature;    // visibility to Radar sensors (0 = invisible, 1 = fully visible)
    public double AcousticSignature; // visibility to Sonar sensors while quiet/passive
}
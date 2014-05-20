using System.Windows.Media.Media3D;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Die Klasse Ball stellt eine Ballform dar, welche zur Darstellung von Geräten und zur
    ///     Berechnung der Kollosionen beim Zeigen des Benutzers verwendet wird.
    ///     @author Florian Kinn
    /// </summary>
    public class Ball
    {
        /// <summary>
        ///     Konstruktur der Klasse Ball
        ///     <param name="centre">Ortsvektor des Mittelpunktes der Kugel</param>
        ///     <param name="radius">Raidus der Kugel</param>
        /// </summary>
        public Ball(Vector3D centre, float radius)
        {
            Centre = centre;
            Radius = radius;
        }

        /// <summary>
        ///     Der Vektor zum Zentrum der Ballform.
        ///     Mit der "set"-Methode kann der Vektor zum Zentrum der Ballform gesetzt werden.
        ///     Mit der "get"-Methode kann der Vektor zum Zentrum der Ballform zurückgegeben werden.
        ///     <returns>Gibt den Vektor zum Zentrum zurück</returns>
        /// </summary>
        public Vector3D Centre { get; set; }

        /// <summary>
        ///     Der Radius der Ballform.
        ///     Mit der "set"-Methode kann der Ballradius gesetzt werden.
        ///     Mit der "get"-Methode kann der Ballradius zurückgegeben werden.
        ///     <returns>Gibt den Radius des Balls zurück</returns>
        /// </summary>
        public float Radius { get; set; }
    }
}
using System;
using RichardPieterse;
using UnityEngine;

[ExecuteInEditMode]
public class Capsule : MonoBehaviour
{
    [ SerializeField] private Capsule _attachedTo;
    
    const int latitudes = 16;
    const int longitudes = 16;
    const int rings = 0;
     public float length = 1;
    public float radius = 1;
    public Vector3 endFulcrumendFulcrum => transform.TransformPoint(Vector3.forward * length);

    
    
    private void OnValidate()
    {
        CombineInstance[] combine = new CombineInstance[1];

        combine[0].mesh = CapsuleData();

        combine[0].transform = Matrix4x4.Rotate(Quaternion.Euler(0, 90,90)) * Matrix4x4.Translate(Vector3.up * length/2);

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
        gameObject.GetOrAddComponent<MeshFilter>().mesh = combinedMesh;

        CapsuleCollider collider = gameObject.GetOrAddComponent<CapsuleCollider>();
        collider.direction = 2;
        collider.radius = radius ;
        collider.height = length + radius*2;
        collider.center = Vector3.forward * length/2;

         gameObject.GetOrAddComponent<MeshRenderer>();

        
    }

    private void Update()
    {
        if (Application.isPlaying == false)
        {
            if (_attachedTo)
            {
                transform.position = _attachedTo.endFulcrumendFulcrum;
            }
        }
    }

    Mesh CapsuleData ( )
    {
        bool calcMiddle = rings > 0;
        int halfLats = latitudes / 2;
        int halfLatsn1 = halfLats - 1;
        int halfLatsn2 = halfLats - 2;
        int ringsp1 = rings + 1;
        int lonsp1 = longitudes + 1;
        float halfDepth = length * 0.5f;
        float summit = halfDepth + radius;

        // Vertex index offsets.
        int vertOffsetNorthHemi = longitudes;
        int vertOffsetNorthEquator = vertOffsetNorthHemi + lonsp1 * halfLatsn1;
        int vertOffsetCylinder = vertOffsetNorthEquator + lonsp1;
        int vertOffsetSouthEquator = calcMiddle ?
            vertOffsetCylinder + lonsp1 * rings :
            vertOffsetCylinder;
        int vertOffsetSouthHemi = vertOffsetSouthEquator + lonsp1;
        int vertOffsetSouthPolar = vertOffsetSouthHemi + lonsp1 * halfLatsn2;
        int vertOffsetSouthCap = vertOffsetSouthPolar + lonsp1;

        // Initialize arrays.
        int vertLen = vertOffsetSouthCap + longitudes;
        Vector3[ ] vs = new Vector3[vertLen];
        Vector2[ ] vts = new Vector2[vertLen];
        Vector3[ ] vns = new Vector3[vertLen];

        float toTheta = 2.0f * Mathf.PI / longitudes;
        float toPhi = Mathf.PI / latitudes;
        float toTexHorizontal = 1.0f / longitudes;
        float toTexVertical = 1.0f / halfLats;

        // Calculate positions for texture coordinates vertical.
        float vtAspectRatio = 1.0f;
        // switch (profile)
        // {
            // case UvProfile.Aspect:
                vtAspectRatio = radius / (length + radius + radius);
                // break;

            // case UvProfile.Uniform:
                // vtAspectRatio = (float) halfLats / (ringsp1 + latitudes);
                // break;

            // case UvProfile.Fixed:
            // default:
                // vtAspectRatio = 1.0f / 3.0f;
                // break;
        // }

        float vtAspectNorth = 1.0f - vtAspectRatio;
        float vtAspectSouth = vtAspectRatio;

        Vector2[ ] thetaCartesian = new Vector2[longitudes];
        Vector2[ ] rhoThetaCartesian = new Vector2[longitudes];
        float[ ] sTextureCache = new float[lonsp1];

        // Polar vertices.
        for (int j = 0; j < longitudes; ++j)
        {
            float jf = j;
            float sTexturePolar = 1.0f - ((jf + 0.5f) * toTexHorizontal);
            float theta = jf * toTheta;

            float cosTheta = Mathf.Cos (theta);
            float sinTheta = Mathf.Sin (theta);

            thetaCartesian[j] = new Vector2 (cosTheta, sinTheta);
            rhoThetaCartesian[j] = new Vector2 (
                radius * cosTheta,
                radius * sinTheta);

            // North.
            vs[j] = new Vector3 (0.0f, summit, 0.0f);
            vts[j] = new Vector2 (sTexturePolar, 1.0f);
            vns[j] = new Vector3 (0.0f, 1.0f, 0f);

            // South.
            int idx = vertOffsetSouthCap + j;
            vs[idx] = new Vector3 (0.0f, -summit, 0.0f);
            vts[idx] = new Vector2 (sTexturePolar, 0.0f);
            vns[idx] = new Vector3 (0.0f, -1.0f, 0.0f);
        }

        // Equatorial vertices.
        for (int j = 0; j < lonsp1; ++j)
        {
            float sTexture = 1.0f - j * toTexHorizontal;
            sTextureCache[j] = sTexture;

            // Wrap to first element upon reaching last.
            int jMod = j % longitudes;
            Vector2 tc = thetaCartesian[jMod];
            Vector2 rtc = rhoThetaCartesian[jMod];

            // North equator.
            int idxn = vertOffsetNorthEquator + j;
            vs[idxn] = new Vector3 (rtc.x, halfDepth, -rtc.y);
            vts[idxn] = new Vector2 (sTexture, vtAspectNorth);
            vns[idxn] = new Vector3 (tc.x, 0.0f, -tc.y);

            // South equator.
            int idxs = vertOffsetSouthEquator + j;
            vs[idxs] = new Vector3 (rtc.x, -halfDepth, -rtc.y);
            vts[idxs] = new Vector2 (sTexture, vtAspectSouth);
            vns[idxs] = new Vector3 (tc.x, 0.0f, -tc.y);
        }

        // Hemisphere vertices.
        for (int i = 0; i < halfLatsn1; ++i)
        {
            float ip1f = i + 1.0f;
            float phi = ip1f * toPhi;

            // For coordinates.
            float cosPhiSouth = Mathf.Cos (phi);
            float sinPhiSouth = Mathf.Sin (phi);

            // Symmetrical hemispheres mean cosine and sine only needs
            // to be calculated once.
            float cosPhiNorth = sinPhiSouth;
            float sinPhiNorth = -cosPhiSouth;

            float rhoCosPhiNorth = radius * cosPhiNorth;
            float rhoSinPhiNorth = radius * sinPhiNorth;
            float zOffsetNorth = halfDepth - rhoSinPhiNorth;

            float rhoCosPhiSouth = radius * cosPhiSouth;
            float rhoSinPhiSouth = radius * sinPhiSouth;
            float zOffsetSouth = -halfDepth - rhoSinPhiSouth;

            // For texture coordinates.
            float tTexFac = ip1f * toTexVertical;
            float cmplTexFac = 1.0f - tTexFac;
            float tTexNorth = cmplTexFac + vtAspectNorth * tTexFac;
            float tTexSouth = cmplTexFac * vtAspectSouth;

            int iLonsp1 = i * lonsp1;
            int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
            int vertCurrLatSouth = vertOffsetSouthHemi + iLonsp1;

            for (int j = 0; j < lonsp1; ++j)
            {
                int jMod = j % longitudes;

                float sTexture = sTextureCache[j];
                Vector2 tc = thetaCartesian[jMod];

                // North hemisphere.
                int idxn = vertCurrLatNorth + j;
                vs[idxn] = new Vector3 (
                    rhoCosPhiNorth * tc.x,
                    zOffsetNorth,
                    -rhoCosPhiNorth * tc.y);
                vts[idxn] = new Vector2 (sTexture, tTexNorth);
                vns[idxn] = new Vector3 (
                    cosPhiNorth * tc.x,
                    -sinPhiNorth,
                    -cosPhiNorth * tc.y);

                // South hemisphere.
                int idxs = vertCurrLatSouth + j;
                vs[idxs] = new Vector3 (
                    rhoCosPhiSouth * tc.x,
                    zOffsetSouth,
                    -rhoCosPhiSouth * tc.y);
                vts[idxs] = new Vector2 (sTexture, tTexSouth);
                vns[idxs] = new Vector3 (
                    cosPhiSouth * tc.x,
                    -sinPhiSouth,
                    -cosPhiSouth * tc.y);
            }
        }

        // Cylinder vertices.
        if (calcMiddle)
        {
            // Exclude both origin and destination edges
            // (North and South equators) from the interpolation.
            float toFac = 1.0f / ringsp1;
            int idxCylLat = vertOffsetCylinder;

            for (int h = 1; h < ringsp1; ++h)
            {
                float fac = h * toFac;
                float cmplFac = 1.0f - fac;
                float tTexture = cmplFac * vtAspectNorth + fac * vtAspectSouth;
                float z = halfDepth - length * fac;

                for (int j = 0; j < lonsp1; ++j)
                {
                    int jMod = j % longitudes;
                    Vector2 tc = thetaCartesian[jMod];
                    Vector2 rtc = rhoThetaCartesian[jMod];
                    float sTexture = sTextureCache[j];

                    vs[idxCylLat] = new Vector3 (rtc.x, z, -rtc.y);
                    vts[idxCylLat] = new Vector2 (sTexture, tTexture);
                    vns[idxCylLat] = new Vector3 (tc.x, 0.0f, -tc.y);

                    ++idxCylLat;
                }
            }
        }

        // Triangle indices.
        // Stride is 3 for polar triangles;
        // stride is 6 for two triangles forming a quad.
        int lons3 = longitudes * 3;
        int lons6 = longitudes * 6;
        int hemiLons = halfLatsn1 * lons6;

        int triOffsetNorthHemi = lons3;
        int triOffsetCylinder = triOffsetNorthHemi + hemiLons;
        int triOffsetSouthHemi = triOffsetCylinder + ringsp1 * lons6;
        int triOffsetSouthCap = triOffsetSouthHemi + hemiLons;

        int fsLen = triOffsetSouthCap + lons3;
        int[ ] tris = new int[fsLen];

        // Polar caps.
        for (int i = 0, k = 0, m = triOffsetSouthCap; i < longitudes; ++i, k += 3, m += 3)
        {
            // North.
            tris[k] = i;
            tris[k + 1] = vertOffsetNorthHemi + i;
            tris[k + 2] = vertOffsetNorthHemi + i + 1;

            // South.
            tris[m] = vertOffsetSouthCap + i;
            tris[m + 1] = vertOffsetSouthPolar + i + 1;
            tris[m + 2] = vertOffsetSouthPolar + i;
        }

        // Hemispheres.
        for (int i = 0, k = triOffsetNorthHemi, m = triOffsetSouthHemi; i < halfLatsn1; ++i)
        {
            int iLonsp1 = i * lonsp1;

            int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
            int vertNextLatNorth = vertCurrLatNorth + lonsp1;

            int vertCurrLatSouth = vertOffsetSouthEquator + iLonsp1;
            int vertNextLatSouth = vertCurrLatSouth + lonsp1;

            for (int j = 0; j < longitudes; ++j, k += 6, m += 6)
            {
                // North.
                int north00 = vertCurrLatNorth + j;
                int north01 = vertNextLatNorth + j;
                int north11 = vertNextLatNorth + j + 1;
                int north10 = vertCurrLatNorth + j + 1;

                tris[k] = north00;
                tris[k + 1] = north11;
                tris[k + 2] = north10;

                tris[k + 3] = north00;
                tris[k + 4] = north01;
                tris[k + 5] = north11;

                // South.
                int south00 = vertCurrLatSouth + j;
                int south01 = vertNextLatSouth + j;
                int south11 = vertNextLatSouth + j + 1;
                int south10 = vertCurrLatSouth + j + 1;

                tris[m] = south00;
                tris[m + 1] = south11;
                tris[m + 2] = south10;

                tris[m + 3] = south00;
                tris[m + 4] = south01;
                tris[m + 5] = south11;
            }
        }

        // Cylinder.
        for (int i = 0, k = triOffsetCylinder; i < ringsp1; ++i)
        {
            int vertCurrLat = vertOffsetNorthEquator + i * lonsp1;
            int vertNextLat = vertCurrLat + lonsp1;

            for (int j = 0; j < longitudes; ++j, k += 6)
            {
                int cy00 = vertCurrLat + j;
                int cy01 = vertNextLat + j;
                int cy11 = vertNextLat + j + 1;
                int cy10 = vertCurrLat + j + 1;

                tris[k] = cy00;
                tris[k + 1] = cy11;
                tris[k + 2] = cy10;

                tris[k + 3] = cy00;
                tris[k + 4] = cy01;
                tris[k + 5] = cy11;
            }
        }

        Mesh mesh = new Mesh ( );
        mesh.vertices = vs;
        mesh.uv = vts;
        mesh.normals = vns;

        // Triangles must be assigned last.
        mesh.triangles = tris;
        mesh.RecalculateTangents ( );
        mesh.Optimize ( );
        return mesh;
    }
    }

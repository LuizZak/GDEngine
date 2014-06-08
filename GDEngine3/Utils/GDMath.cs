using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace GDEngine3.Utils
{
    /// <summary>
    /// Provides useful math functions
    /// </summary>
    public static class GDMath
    {
        /// <summary>
        /// Random number generator
        /// </summary>
        public static Random r = new Random();

        /// <summary>
        /// Returns a random number in the range of [0-1[
        /// </summary>
        /// <returns>A random number in the range of [0-1[</returns>
        public static float random()
        {
            return (float)r.NextDouble();
        }

        /// <summary>
        /// Returns true at a chance of 1 out of nChance
        /// </summary>
        /// <param name="nChance">The chance of returning true in 1 out of nChance. Larger numbers will lower the chances of returning true</param>
        /// <returns>True at a chance of 1 out of nChance</returns>
        public static bool Chance(int nChance)
        {
            return (random() < 1.0f / nChance);
        }

        /// <summary>
        /// Returns the distance between the two given entities
        /// </summary>
        /// <param name="entity1">A valid GDEntity that has the same parent as the second entity</param>
        /// <param name="entity2">A valid GDEntity that has the same parent as the first entity</param>
        /// <returns>The distance between the given entities</returns>
        public static float Distance(GDEntity entity1, GDEntity entity2)
        {
            return Vector2.Distance(entity1.position, entity2.position);
        }

        /// <summary>
        /// Returns the distance between two points
        /// </summary>
        /// <param name="x1">The X vertice of the first point</param>
        /// <param name="y1">The Y vertice of the first point</param>
        /// <param name="x2">The X vertice of the second point</param>
        /// <param name="y2">The Y vertice of the second point</param>
        /// <returns>The distance between the two given points</returns>
        public static float Distance(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        /// <summary>
        /// Returns the distance between two Vector2 objects
        /// </summary>
        /// <param name="vec1">The first Vector2 used to calculate the distance</param>
        /// <param name="vec2">The second Vector2 used to calculate the distance</param>
        /// <returns>The distance between the two vectors</returns>
        public static float Distance(Vector2 vec1, Vector2 vec2)
        {
            return (float)Math.Sqrt((vec1.X - vec2.X) * (vec1.X - vec2.X) + (vec1.Y - vec2.Y) * (vec1.Y - vec2.Y));
        }

        /// <summary>
        /// Creates a set of points that forms a free-transformed rectangle off the given settings
        /// </summary>
        /// <param name="Pos">The rectangle's position</param>
        /// <param name="Size">The rectangle's size</param>
        /// <param name="Offset">The rectangle's offset</param>
        /// <param name="Scale">The rectangle's scale</param>
        /// <param name="Rotation">The rectangle's rotation</param>
        /// <returns>A set of points that forms a free-transformed rectangle off the given settings</returns>
        public static Vector2[] GetRectangle(Vector2 Pos, Vector2 Size, Vector2 Offset, Vector2 Scale, float Rotation)
        {
            // Create the point set
            Vector2[] points = new Vector2[4];
            
            // Position this point set around
            points[0] = new Vector2((-Offset.X)         * Scale.X, (-Offset.Y)         * Scale.Y);
            points[1] = new Vector2((Size.X - Offset.X) * Scale.X, (-Offset.Y)         * Scale.Y);
            points[2] = new Vector2((Size.X - Offset.X) * Scale.X, (Size.Y - Offset.Y) * Scale.Y);
            points[3] = new Vector2((-Offset.X)         * Scale.X, (Size.Y - Offset.Y) * Scale.Y);

            // Do a little check here, if the rotation is equal to 0, we skip a calculation already
            if (Rotation % (Math.PI * 2) == 0)
            {
                // Sum the points with the position
                points[0] = Pos + points[0];
                points[1] = Pos + points[1];
                points[2] = Pos + points[2];
                points[3] = Pos + points[3];
            }
            else
            {
                // Rotate and then sum the points with the position
                points[0] = Pos + Rotate(points[0], Rotation);
                points[1] = Pos + Rotate(points[1], Rotation);
                points[2] = Pos + Rotate(points[2], Rotation);
                points[3] = Pos + Rotate(points[3], Rotation);
            }

            return points;
        }

        /// <summary>
        /// Gets the bounding box of the given point set
        /// </summary>
        /// <param name="pointsSet">The point set to make the bounding box out of</param>
        /// <returns>The AABB bounding box for the given points</returns>
        public static RectangleF GetBoundingBox(Vector2[] pointsSet)
        {
            float maxX = 0, maxY = 0, minX = 0, minY = 0;

            if (pointsSet.Length == 4)
            {
                maxX = Math.Max(Math.Max(Math.Max(pointsSet[0].X, pointsSet[1].X), pointsSet[2].X), pointsSet[3].X);
                maxY = Math.Max(Math.Max(Math.Max(pointsSet[0].Y, pointsSet[1].Y), pointsSet[2].Y), pointsSet[3].Y);

                minX = Math.Min(Math.Min(Math.Min(pointsSet[0].X, pointsSet[1].X), pointsSet[2].X), pointsSet[3].X);
                minY = Math.Min(Math.Min(Math.Min(pointsSet[0].Y, pointsSet[1].Y), pointsSet[2].Y), pointsSet[3].Y);
            }
            else if (pointsSet.Length == 2)
            {
                maxX = Math.Max(pointsSet[0].X, pointsSet[1].X);
                maxY = Math.Max(pointsSet[0].Y, pointsSet[1].Y);

                minX = Math.Min(pointsSet[0].X, pointsSet[1].X);
                minY = Math.Min(pointsSet[0].Y, pointsSet[1].Y);
            }
            else
            {
                for (int i = 0; i < pointsSet.Length; i++)
                {
                    maxX = Math.Max(pointsSet[i].X, maxX);
                    maxY = Math.Max(pointsSet[i].Y, maxY);

                    minX = Math.Min(pointsSet[i].X, maxX);
                    minY = Math.Min(pointsSet[i].Y, maxY);
                }
            }

            return new RectangleF(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Gets the bounding box of the given point set
        /// </summary>
        /// <param name="point1">The first point</param>
        /// <param name="point1">The second point</param>
        /// <returns>The AABB bounding box for the given points</returns>
        public static RectangleF GetBoundingBox(Vector2 point1, Vector2 point2)
        {
            float maxX = 0, maxY = 0, minX = 0, minY = 0;

            maxX = Math.Max(point1.X, point2.X);
            maxY = Math.Max(point1.Y, point2.Y);

            minX = Math.Min(point1.X, point2.X);
            minY = Math.Min(point1.Y, point2.Y);

            return new RectangleF(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Returns a valid RectangleF with corrected width and height based on the specified points
        /// </summary>
        /// <param name="pointsSet">The points set to convert to a RectangleF. No sorting is necessary</param>
        /// <returns>A RectangleF based on the total area of the given points</returns>
        public static RectangleF GetRectangleArea(Vector2[] pointsSet)
        {
            RectangleF f = GetBoundingBox(pointsSet);

            f.Width = Math.Abs(f.X - f.Width);
            f.Height = Math.Abs(f.Y - f.Height);

            return f;
        }

        /// <summary>
        /// Returns a valid RectangleF with corrected width and height based on the specified points
        /// </summary>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>A RectangleF based on the total area of the given points</returns>
        public static RectangleF GetRectangleArea(Vector2 point1, Vector2 point2)
        {
            RectangleF f = GetBoundingBox(point1, point2);

            f.Width = Math.Abs(f.X - f.Width);
            f.Height = Math.Abs(f.Y - f.Height);

            return f;
        }

        /// <summary>
        /// Transforms a set of points based on the given transformation parameters directly into another set of points
        /// </summary>
        /// <param name="points">The set of points to transform</param>
        /// <param name="toPoints">The target set of points to transform</param>
        /// <param name="offset">The offset to apply to the points</param>
        /// <param name="origin">The origin to apply to the points before rotating</param>
        /// <param name="scale">The scale to apply to the points</param>
        /// <param name="rotation">The rotation to apply to the points</param>
	    public static void TransformPoints(Vector2[] points, Vector2[] toPoints, Vector2 offset, Vector2 origin, Vector2 scale, float rotation)
	    {
		    for(int i = 0; i < points.Length; i++)
		    {
			    toPoints[i] = points[i] - origin;
			    toPoints[i] *= scale;

			    if(rotation != 0)
				    toPoints[i] = offset + Rotate(toPoints[i], rotation);
			    else
				    toPoints[i] = offset + toPoints[i];
		    }
	    }

        /// <summary>
        /// Rotates a point around by the given radians
        /// </summary>
        /// <param name="p">The point to rotate</param>
        /// <param name="rad">The rotation amount, in radians</param>
        /// <returns>The point, rotated by the given radians</returns>
        public static Vector2 Rotate(Vector2 p, float rad)
        {
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

	        return new Vector2(p.X * cos - p.Y * sin, p.Y * cos + p.X * sin);
        }

        /// <summary>
        /// Tests whether a point is inside a polygon
        /// </summary>
        /// <param name="point">The point to test for</param>
        /// <param name="polygon">The polygon points to test for</param>
        /// <returns>Whether the point is inside the polygon</returns>
        public static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            Vector2 endPt;
            endPt.X = GetRectangleArea(polygon).Right + 1;
            endPt.Y = point.Y;

            // line we are testing against goes from pt -> endPt.
            bool inside = false;

            Vector2 edgeSt = polygon[0];
            Vector2 edgeEnd;

            for (int i = 0; i < polygon.Length; i++)
            {
                // the current edge is defined as the line from edgeSt -> edgeEnd.
                edgeEnd = polygon[(i + 1) % polygon.Length];

                // perform check now...
                if (((edgeSt.Y <= point.Y) && (edgeEnd.Y > point.Y)) || ((edgeSt.Y > point.Y) && (edgeEnd.Y <= point.Y)))
                {
                    // this line crosses the test line at some point... does it do so within our test range?
                    float slope = (edgeEnd.X - edgeSt.X) / (edgeEnd.Y - edgeSt.Y);
                    float hitX = edgeSt.X + ((point.Y - edgeSt.Y) * slope);

                    if ((hitX >= point.X) && (hitX <= endPt.X))
                        inside = !inside;
                }
                edgeSt = edgeEnd;
            }

            return inside;
        }

        /// <summary>
        /// Returns whether the two given line segments intersect
        /// </summary>
        /// <param name="ptA">The first segment's start point</param>
        /// <param name="ptB">The first segment's end point</param>
        /// <param name="ptC">The second segment's start point</param>
        /// <param name="ptD">The second segment's end point</param>
        /// <returns>Whether the two given segments intersect</returns>
        public static bool LinesIntersect(Vector2 ptA, Vector2 ptB, Vector2 ptC, Vector2 ptD)
        {
            float r, s, d;
			
            float x1 = ptA.X, y1 = ptA.Y,
				x2 = ptB.X, y2 = ptB.Y,
				x3 = ptC.X, y3 = ptC.Y,
				x4 = ptD.X, y4 = ptD.Y;

            //Make sure the lines aren't parallel
            if ((y2 - y1) / (x2 - x1) != (y4 - y3) / (x4 - x3))
            {
                d = (((x2 - x1) * (y4 - y3)) - (y2 - y1) * (x4 - x3));
                if (d != 0)
                {
                    r = (((y1 - y3) * (x4 - x3)) - (x1 - x3) * (y4 - y3)) / d;
                    s = (((y1 - y3) * (x2 - x1)) - (x1 - x3) * (y2 - y1)) / d;
                    if (r >= 0 && r <= 1)
                    {
						if (s >= 0 && s <= 1)
                        {
                            return true;
                        }
						else return false;
                    }
					else return false;
                }
            }
			
			return false;
        }

        /// <summary>
        /// Checks if two lines intersect, and return the intersection result
        /// </summary>
        /// <param name="line1Start">The starting point of the first line</param>
        /// <param name="line1End">The ending point of the first line</param>
        /// <param name="line2Start">The starting point of the second line</param>
        /// <param name="line2End">The ending point of the second line</param>
        /// <returns>A LineCollisionResult compounding the result of the line intersection</returns>
        public static LineCollisionResult LinesIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
        {
			float x1 = line1Start.X;
            float y1 = line1Start.Y;

            float x2 = line1End.X;
            float y2 = line1End.Y;

            float x3 = line2Start.X;
            float y3 = line2Start.Y;

            float x4 = line2End.X;
            float y4 = line2End.Y;

            
            float r = 0;
            float s = 0;
            float d = 0;
			
            //Make sure the lines aren't parallel
            if ((y2 - y1) / (x2 - x1) != (y4 - y3) / (x4 - x3))
            {
                d = (((x2 - x1) * (y4 - y3)) - (y2 - y1) * (x4 - x3));
                if (d != 0)
                {
                    r = (((y1 - y3) * (x4 - x3)) - (x1 - x3) * (y4 - y3)) / d;
                    s = (((y1 - y3) * (x2 - x1)) - (x1 - x3) * (y2 - y1)) / d;
                    if (r >= 0 && r <= 1)
                    {
						if (s >= 0 && s <= 1)
                        {
                            return new LineCollisionResult() { Intersect = true, IntersectionPoint = new Vector2(x1 + r * (x2 - x1), y1 + r * (y2 - y1)), Line1Ratio = r, Line2Ratio = s };
                        }
						else
                            return new LineCollisionResult() { Intersect = false, IntersectionPoint = Vector2.Zero, Line1Ratio = 0, Line2Ratio = 0 };
                    }
                    else
                        return new LineCollisionResult() { Intersect = false, IntersectionPoint = Vector2.Zero, Line1Ratio = 0, Line2Ratio = 0 };
                }
            }

            return new LineCollisionResult() { Intersect = false, IntersectionPoint = Vector2.Zero, Line1Ratio = 0, Line2Ratio = 0 };
        }

        /// <summary>
        /// Line intersection test result
        /// </summary>
        public struct LineCollisionResult
        {
            /// <summary>
            /// Whether the lines intersect
            /// </summary>
            public bool Intersect;

            /// <summary>
            /// The point at which the lines intersect
            /// </summary>
            public Vector2 IntersectionPoint;

            /// <summary>
            /// A float value between [0 - 1] that specifies at which magnitude on the first line the intersection point lies
            /// </summary>
            public float Line1Ratio;

            /// <summary>
            /// A float value between [0 - 1] that specifies at which magnitude on the second line the intersection point lies
            /// </summary>
            public float Line2Ratio;
        }


        /// Code by PogoPixels, implementation by Luiz Fernando

        public struct PolygonCollisionResult
        {
            /// <summary>
            /// Whether the polygons will intersect in a future timestep
            /// </summary>
            public bool WillIntersect; // Are the polygons going to intersect forward in time?
            /// <summary>
            /// Whether the polygons are currently intersecting
            /// </summary>
            public bool Intersect; // Are the polygons currently intersecting
            /// <summary>
            /// The translation to apply to the polygon to cancel the penetration
            /// </summary>
            public Vector2 MinimumTranslationVector; // The translation to apply to polygon A to push the polygons appart.
        }

        /// <summary>
        /// Check if polygon A is going to collide with polygon B for the given velocity
        /// </summary>
        /// <param name="polygonA">The first polygon to check</param>
        /// <param name="polygonB">The second polygon to check</param>
        /// <param name="resolve">Whether to resolve the collisions using the static polygon references</param>
        /// <returns>A PolygonCollisionResult specifying the result of this test</returns>
        public static PolygonCollisionResult PolygonCollision(Vector2[] polygonA, Vector2[] polygonB, bool resolve = false)
        {
            PolygonCollisionResult result = new PolygonCollisionResult();
            result.Intersect = true;
            result.WillIntersect = false;

            int edgeCountA = polygonA.Length;
            int edgeCountB = polygonB.Length;
            float minIntervalDistance = float.PositiveInfinity;
            Vector2 translationAxis = Vector2.Zero;
            Vector2 edge;

            Vector2 center1 = Vector2.Zero;
            Vector2 center2 = Vector2.Zero;

            if (resolve)
            {
                // Calculate the center of the polygons
                for (var i = 0; i < polygonA.Length; i += 1)
                {
                    center1 += polygonA[i];
                }
                center1 = center1 / polygonA.Length;

                for (var i = 0; i < polygonB.Length; i += 1)
                {
                    center2 += polygonB[i];
                }
                center2 = center2 / polygonB.Length;
            }

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = (polygonA[edgeIndex] - polygonA[(edgeIndex + 1) % polygonA.Length]);
                }
                else
                {
                    edge = (polygonB[(edgeIndex - edgeCountA)] - polygonB[((edgeIndex + 1) - edgeCountA) % polygonB.Length]);
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0) result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====


                // Project the velocity on the current axis

                float velocityProjection = Vector2.Dot(axis, Vector2.Zero);

                // Get the projection of polygon A during the movement

                if (velocityProjection < 0)
                {
                    minA += velocityProjection;
                }
                else
                {
                    maxA += velocityProjection;
                }

                // Do the same test as above for the new projection

                float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop

                if (!result.Intersect && !result.WillIntersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector

                if (resolve)
                {
                    intervalDistance = Math.Abs(intervalDistance);
                    if (intervalDistance < minIntervalDistance)
                    {
                        minIntervalDistance = intervalDistance;
                        translationAxis = axis;

                        Vector2 d = center1 - center2;
                        if (Vector2.Dot(d, translationAxis) < 0)
                            translationAxis = -translationAxis;
                    }
                }
            }

            if(resolve)
                result.MinimumTranslationVector = translationAxis * minIntervalDistance;

            return result;
        }

        /// <summary>
        /// Calculate the distance between [minA, maxA] and [minB, maxB]
        /// The distance will be negative if the intervals overlap
        /// </summary>
        /// <param name="minA">The first interval's starting point</param>
        /// <param name="maxA">The first interval's ending point</param>
        /// <param name="minB">The second interval's starting point</param>
        /// <param name="maxB">The second interval's ending point</param>
        /// <returns>The distance between the intervals (negative if the intervals overlap)</returns>
        public static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }

        /// <summary>
        /// Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        /// </summary>
        /// <param name="axis">The axis to project</param>
        /// <param name="polygon">The polygon to test against</param>
        /// <param name="min">The resulting minimum overlap</param>
        /// <param name="max">The resulting maximum overlap</param>
        public static void ProjectPolygon(Vector2 axis, Vector2[] polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float d = Vector2.Dot(axis, polygon[0]);

            min = d;
            max = d;
            for (int i = 0; i < polygon.Length; i++)
            {
                d = Vector2.Dot(axis, polygon[i]);

                if (d < min)
                {
                    min = d;
                }
                else
                {
                    if (d > max)
                    {
                        max = d;
                    }
                }
            }
        }
    }
}

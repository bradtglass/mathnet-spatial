﻿using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics;
using MathNet.Spatial.Units;

namespace MathNet.Spatial.Euclidean
{
    /// <summary>
    /// A line between two points
    /// </summary>
    [Serializable]
    public struct Line3D : IEquatable<Line3D>, IXmlSerializable
    {
        /// <summary>
        /// The startpoint of the line
        /// </summary>
        public readonly Point3D StartPoint;

        /// <summary>
        /// The endpoint of the line
        /// </summary>
        public readonly Point3D EndPoint;
        private double _length;
        private UnitVector3D _direction;

        /// <summary>
        /// Throws if StartPoint == EndPoint
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public Line3D(Point3D startPoint, Point3D endPoint)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            if (this.StartPoint == this.EndPoint)
            {
                throw new ArgumentException("StartPoint == EndPoint");
            }

            this._length = -1.0;
            this._direction = new UnitVector3D();
        }

        /// <summary>
        /// Distance from startpoint to endpoint, the length of the line
        /// </summary>
        public double Length
        {
            get
            {
                if (this._length < 0)
                {
                    var vectorTo = this.StartPoint.VectorTo(this.EndPoint);
                    this._length = vectorTo.Length;
                    if (this._length > 0)
                    {
                        this._direction = vectorTo.Normalize();
                    }
                }

                return this._length;
            }
        }

        /// <summary>
        /// The direction from the startpoint to the endpoint
        /// </summary>
        public UnitVector3D Direction
        {
            get
            {
                if (this._length < 0)
                {
                    this._length = this.Length; // Side effect hack
                }

                return this._direction;
            }
        }

        /// <summary>
        /// Creates a Line from its string representation
        /// </summary>
        /// <param name="startPoint">The string representation of the startpoint</param>
        /// <param name="endPoint">The string representation of the endpoint</param>
        /// <returns></returns>
        public static Line3D Parse(string startPoint, string endPoint)
        {
            return new Line3D(Point3D.Parse(startPoint), Point3D.Parse(endPoint));
        }

        public static bool operator ==(Line3D left, Line3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Line3D left, Line3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the shortest line to a point
        /// </summary>
        /// <param name="p"></param>
        /// <param name="mustStartBetweenStartAndEnd">If false the startpoint can be on the line extending beyond the start and endpoint of the line</param>
        /// <returns></returns>
        public Line3D LineTo(Point3D p, bool mustStartBetweenStartAndEnd)
        {
            return new Line3D(this.ClosestPointTo(p, mustStartBetweenStartAndEnd), p);
        }

        /// <summary>
        /// Returns the closest point on the line to the given point.
        /// </summary>
        /// <param name="p">The point which the returned point is the closest point on the line to</param>
        /// <param name="mustBeOnSegment">If true the returned point is contained by the segment ends, otherwise it can be anywhere on the projected line.</param>
        /// <returns></returns>
        public Point3D ClosestPointTo(Point3D p, bool mustBeOnSegment)
        {
            Vector3D v = (p - this.StartPoint);
            double dotProduct = v.DotProduct(this.Direction);
            if (mustBeOnSegment)
            {
                if (dotProduct < 0)
                    dotProduct = 0;

                if (dotProduct > this.Length)
                    dotProduct = this.Length;
            }

            Vector3D alongVector = dotProduct*this.Direction;
            return this.StartPoint + alongVector;
        }
        

        /// <summary>
        /// The line projected on a plane
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public Line3D ProjectOn(Plane plane)
        {
            return plane.Project(this);
        }

        /// <summary>
        /// Find the intersection between the line and a plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public Point3D? IntersectionWith(Plane plane, double tolerance = double.Epsilon)
        {
            return plane.IntersectionWith(this, tolerance);
        }

        /// <summary>
        /// Checks to determine whether or not two lines are parallel to each other, using the dot product within 
        /// the double precision specified in the MathNet.Numerics package.
        /// </summary>
        /// <param name="other">The other line to check this one against</param>
        /// <returns>True if the lines are parallel, false if they are not</returns>
        public bool IsParallelTo(Line3D other)
        {
            return this.Direction.IsParallelTo(other.Direction, Precision.DoublePrecision * 2);
        }

        /// <summary>
        /// Checks to determine whether or not two lines are parallel to each other within a specified angle tolerance
        /// </summary>
        /// <param name="other">The other line to check this one against</param>
        /// <param name="angleTolerance">If the angle between line directions is less than this value, the method returns true</param>
        /// <returns>True if the lines are parallel within the angle tolerance, false if they are not</returns>
        public bool IsParallelTo(Line3D other, Angle angleTolerance)
        {
            return this.Direction.IsParallelTo(other.Direction, angleTolerance);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Line3D other)
        {
            return this.StartPoint.Equals(other.StartPoint) && this.EndPoint.Equals(other.EndPoint);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Line3D && this.Equals((Line3D)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.StartPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ this.EndPoint.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("StartPoint: {0}, EndPoint: {1}", this.StartPoint, this.EndPoint);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
        
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            var startPoint = Point3D.ReadFrom(e.SingleElement("StartPoint").CreateReader());
            XmlExt.SetReadonlyField(ref this, l => l.StartPoint, startPoint);
            var endPoint = Point3D.ReadFrom(e.SingleElement("EndPoint").CreateReader());
            XmlExt.SetReadonlyField(ref this, l => l.EndPoint, endPoint);
        }
        
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement("StartPoint", this.StartPoint);
            writer.WriteElement("EndPoint", this.EndPoint);
        }
    }
}

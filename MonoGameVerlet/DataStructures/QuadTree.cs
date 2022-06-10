using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGameVerlet.Verlet;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.DataStructures
{
	public class QuadTree
	{
		public int MaxObjects = 50;
		public int MaxLevels = 5;

		private int level;
		private List<VerletComponent> objects; //convert to your object type
		private List<VerletComponent> returnObjects;
		private Rectangle bounds;
		private QuadTree[] nodes;


		public QuadTree(int pLevel, Rectangle pBounds)
		{
			level = pLevel;
			objects = new List<VerletComponent>();
			bounds = pBounds;
			nodes = new QuadTree[4];
			returnObjects = new List<VerletComponent>();
		}

		public void Clear()
		{
			objects.Clear();

			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i] != null)
				{
					nodes[i].Clear();
					nodes[i] = null;
				}
			}
		}

		private void Split()
		{
			float subWidth = bounds.Width / 2;
			float subHeight = bounds.Height / 2;
			float x = bounds.X;
			float y = bounds.Y;

			nodes[0] = new QuadTree(level + 1, new Rectangle((int)(x + subWidth), (int)y, (int)subWidth, (int)subHeight));
			nodes[1] = new QuadTree(level + 1, new Rectangle((int)x, (int)y, (int)subWidth, (int)subHeight));
			nodes[2] = new QuadTree(level + 1, new Rectangle((int)x, (int)(y + subHeight), (int)subWidth, (int)subHeight));
			nodes[3] = new QuadTree(level + 1, new Rectangle((int)(x + subWidth), (int)(y + subHeight), (int)subWidth, (int)subHeight));
		}


		// Determine which node the object belongs to. -1 means
		// object cannot completely fit within a child node and is part
		// of the parent node

		private int GetIndex(Rectangle pRect) //TODO: update this name to a rect
		{
			int index = -1;
			double verticalMidpoint = bounds.X + (bounds.Width / 2f);
			double horizontalMidpoint = bounds.Y + (bounds.Height / 2f);

			// Object can completely fit within the top quadrants
			bool topQuadrant = (pRect.Y < horizontalMidpoint && pRect.Y + pRect.Height < horizontalMidpoint);
			// Object can completely fit within the bottom quadrants
			bool bottomQuadrant = (pRect.Y > horizontalMidpoint);

			// Object can completely fit within the left quadrants
			if (pRect.X < verticalMidpoint && pRect.X + pRect.Width < verticalMidpoint)
			{
				if (topQuadrant)
				{
					index = 1;
				}
				else if (bottomQuadrant)
				{
					index = 2;
				}
			}
			// Object can completely fit within the right quadrants
			else if (pRect.X > verticalMidpoint)
			{
				if (topQuadrant)
				{
					index = 0;
				}
				else if (bottomQuadrant)
				{
					index = 3;
				}
			}

			return index;
		}

		public void Insert(VerletComponent verletComponent)
		{
			if (nodes[0] != null)
			{
				int index = GetIndex(verletComponent.Bounds);

				if (index != -1)
				{
					nodes[index].Insert(verletComponent);

					return;
				}
			}

			objects.Add(verletComponent);

			if (objects.Count > MaxObjects && level < MaxLevels)
			{
				if (nodes[0] == null)
				{
					Split();
				}

				int i = 0;
				while (i < objects.Count)
				{
					int index = GetIndex(objects[i].Bounds);
					if (index != -1)
					{
						nodes[index].Insert(objects[i]);
						objects.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}
			}
		}


		// Return all objects that could collide with the given object (recursive)

		public void Retrieve(List<VerletComponent> returnedVerletComponents, Rectangle rect)
		{
			if (nodes[0] != null)
			{
				var index = GetIndex(rect);
				if (index != -1)
				{
					nodes[index].Retrieve(returnedVerletComponents, rect);
				}
			}
			returnedVerletComponents.AddRange(objects);
		}

		public void Draw(SpriteBatch spriteBatch, GraphicsDevice g)
		{
			foreach (QuadTree node in nodes)
			{
				if (node != null)
				{
					node.Draw(spriteBatch, g);
					ShapeExtensions.DrawRectangle(spriteBatch, new Rectangle(node.bounds.Left, node.bounds.Top, node.bounds.Width, node.bounds.Height), Color.White);
				}
			}
		}

		public void Update(List<VerletComponent> VerletComponents)
		{
			Clear();
			foreach (var m in VerletComponents)
			{
				Insert(m);
			}
		}
	}
}

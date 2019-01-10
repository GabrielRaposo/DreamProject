using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CustomPhysics
{
    //Copiando do tutorial de 2017 fite me
    public class PhysicsObject : MonoBehaviour
    {
        public float minGroundNormalY = .65f;
        public float gravityModifier = 1f;

        protected Vector2 targetVelocity;
        protected bool grounded;
        protected Vector2 groundNormal;

        protected new Rigidbody2D rigidbody2D;
        protected Vector2 velocity;
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = 0.02f;

        private void OnEnable()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            contactFilter.useLayerMask = true;
        }

        void Update()
        {
            targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        protected virtual void ComputeVelocity()
        {

        }

        private void FixedUpdate()
        {
            velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
            velocity.x = targetVelocity.x;

            grounded = false;

            Vector2 deltaPosition = velocity * Time.deltaTime;

            Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

            Vector2 move = moveAlongGround * deltaPosition.x;

            Movement(move, false);

            move = Vector2.up * deltaPosition.y;

            Movement(move, true);
        }

        private void Movement(Vector2 move, bool yMovement)
        {
            float distance = move.magnitude;

            if (distance > minMoveDistance)
            {
                int count = rigidbody2D.Cast(move, contactFilter, hitBuffer, distance + shellRadius); 

                hitBufferList.Clear();
                for (int i = 0; i < count; i++)
                {
                    hitBufferList.Add(hitBuffer[i]);
                }

                for (int i = 0; i < hitBufferList.Count; i++)
                {
                    Vector2 currentNormal = hitBufferList[i].normal;
                    if (currentNormal.y > minGroundNormalY)
                    {
                        grounded = true;
                        if (yMovement)
                        {
                            groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }

                    float projection = Vector2.Dot(velocity, currentNormal);
                    if (projection < 0)
                    {
                        velocity = velocity - projection * currentNormal;
                    }

                    float modifiedDistance = hitBufferList[i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }

            }

            rigidbody2D.position = rigidbody2D.position + move.normalized * distance;
        }

    }
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using MessagePipe;
using UnityEngine;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public sealed class BeginDodgeCreateGhost : MonoBehaviour
    {
        [SerializeField]
        private Actor actor;

        [SerializeField]
        private float createIntervalSeconds;

        [SerializeField]
        private int createNumber;

        [SerializeField]
        private float destroyGhostSeconds;

        [SerializeField]
        private Material ghostMaterial;

        private IDisposable createGhostScope;

        private void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            MessageBroker.GetSubscriber<Actor, ActorEvents.BeginDodge>()
                .Subscribe(this.actor, x =>
                {
                    this.createGhostScope = this.BeginCreateGhost();
                })
                .AddTo(ct);

            MessageBroker.GetSubscriber<Actor, ActorEvents.EndDodge>()
                .Subscribe(this.actor, _ =>
                {
                    this.createGhostScope?.Dispose();
                })
                .AddTo(ct);
        }

        private IDisposable BeginCreateGhost()
        {
            return UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(this.createIntervalSeconds))
                .Take(this.createNumber)
                .Subscribe(_ =>
                {
                    var clones = new List<GameObject>();
                    var material = Instantiate(this.ghostMaterial);
                    foreach (var meshRenderer in this.actor.ModelController.ModelDataHolder.MeshRenderers)
                    {
                        var t = meshRenderer.transform;
                        var clone = Instantiate(meshRenderer, t.position, t.rotation);
                        clones.Add(clone.gameObject);
                        clone.sharedMaterial = material;
                        this.BeginUpdateGhostMaterial(clone);
                    }

                    UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(this.destroyGhostSeconds))
                        .Subscribe(_ =>
                        {
                            foreach (var clone in clones)
                            {
                                Destroy(clone);
                            }
                        });
                });
        }

        private void BeginUpdateGhostMaterial(Renderer renderer)
        {
            var m = renderer.sharedMaterial;
            var tween = DOTween.To(
                () => m.color.a,
                x =>
                {
                    var c = m.color;
                    c.a = x;
                    m.color = c;
                },
                0.0f,
                this.destroyGhostSeconds
                );
            tween.OnComplete(() =>
            {
                Destroy(m);
                Destroy(renderer.gameObject);
            });
        }
    }
}

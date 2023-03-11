using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MutliImageTracking : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager _imageManager;
    [SerializeField] private ARSessionOrigin _sessionOrigin;
    [SerializeField] private GameObject worldOrigin;
    private Coroutine _coroutine;


    private void OnEnable()
    {
        worldOrigin = new GameObject("Origin");
        _imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        _imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private IEnumerator OriginDecide(ARTrackedImage trackedImage, float trackInterval)
    {
        yield return new WaitForSeconds(trackInterval);
        var trackedImageTransform = trackedImage.transform;
        // 画像がワールド空間の原点となる。
        worldOrigin.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        // 原点と画像の位置合わせをしてくれるやつ。ワールド座標の原点を第二引数、第三引数のを反映する。
        _sessionOrigin.MakeContentAppearAt(worldOrigin.transform, trackedImageTransform.position,
            trackedImageTransform.localRotation);
        _coroutine = null;
    }

    private Vector3 WorldToOriginLocal(Vector3 world)
    {
        // ワールド座標を入れてそれをローカル空間の方向ベクトルへ変換する。
        return worldOrigin.transform.InverseTransformDirection(world);
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            StartCoroutine(OriginDecide(trackedImage, 0));
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (_coroutine == null) _coroutine = StartCoroutine(OriginDecide(trackedImage, 5));
        }
    }
}

using UnityEngine;

public class FaceDemo : MonoBehaviour
{
    public Transform head;  
    public float speed = 3f, angryTime = 1f;
    TextMesh tm; float angryUntil;

    void Start()
    {
        if (!GetComponent<Rigidbody>()) { var rb = gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; }
        var host = head ? head : transform;             
        var go = new GameObject("Face"); tm = go.AddComponent<TextMesh>();
        go.transform.SetParent(host, false);
        go.transform.localPosition = new Vector3(0, 0, 0.55f);
        tm.alignment = TextAlignment.Center; tm.anchor = TextAnchor.MiddleCenter;
        tm.fontSize = 100; tm.characterSize = 0.02f; tm.color = Color.black; tm.text = ":|";
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"), v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;
        if (dir.sqrMagnitude > 0) { transform.position += dir * speed * Time.deltaTime; transform.forward = dir; }
        tm.text = Time.time < angryUntil ? ">:(" : (dir.sqrMagnitude > 0 ? ":)" : ":|");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other is BoxCollider || other.CompareTag("Box")) angryUntil = Time.time + angryTime;
    }
}

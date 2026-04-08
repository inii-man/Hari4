curl -c cookies.txt -s -X POST "http://localhost:5253/Auth/Login" \
-H "Content-Type: application/x-www-form-urlencoded" \
-d "Username=test_user_mvc&Password=password123"

echo "FIRST REQUEST:"
curl -b cookies.txt -s -i http://localhost:5253/Product | grep -i "Total <strong"

echo "SECOND REQUEST:"
curl -b cookies.txt -s -i http://localhost:5253/Product | grep -i "Total <strong"
